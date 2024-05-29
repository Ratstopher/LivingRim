import express from 'express';
import bodyParser from 'body-parser';
import fetch from 'node-fetch';
import sqlite3 from 'sqlite3';
import { open } from 'sqlite';
import path from 'path';
import dotenv from 'dotenv';

dotenv.config();

const app = express();
const PORT = 3000;
const __dirname = path.dirname(new URL(import.meta.url).pathname).substring(1);
const DB_PATH = path.join(__dirname, 'chat_log.db');

app.use(bodyParser.json());

let db;

(async () => {
    db = await open({
        filename: DB_PATH,
        driver: sqlite3.Database
    });

    await db.exec(`
        CREATE TABLE IF NOT EXISTS chat_log (
            id INTEGER PRIMARY KEY AUTOINCREMENT,
            CharacterId TEXT,
            Name TEXT,
            Interaction TEXT,
            Content TEXT,
            Timestamp TEXT
        )
    `);
    console.log('Database initialized and chat_log table created');
})();

app.post('/api/v1/chat/completions', async (req, res) => {
    console.log('Received request:', req.body);

    const { characterId, interactions, details } = req.body;

    const prompt = `
    Name: ${details.name}
    Mood: ${details.mood}
    Health: ${details.health}
    Personality: ${details.personality}
    Relationships: ${details.relationships}
    Environment: ${details.environment}
    Needs: ${details.needs}
    Backstory: ${details.backstory}

    The following is a role-play conversation between You and the user. You are ${details.name}, a character in RimWorld. ${details.name} has the personality traits of ${details.personality}. ${details.name}'s backstory so far has been ${details.backstory} and their current mood is ${details.mood}. They have the following relationships: ${details.relationships}. The current environment is as follows: ${details.environment}. The needs of ${details.name} are: ${details.needs}.
    Interaction: ${interactions.join(' ')}
    `;

    const apiKey = process.env.OPENROUTER_API_KEY;

    const requestBody = {
        model: 'mistralai/mistral-7b-instruct:free',
        messages: [
            {
                role: 'assistant',
                content: prompt
            }
        ]
    };

    console.log('Request Body:', requestBody);

    try {
        const response = await fetch('https://openrouter.ai/api/v1/chat/completions', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${apiKey}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody)
        });

        const data = await response.json();
        console.log('Response from API:', data);

        if (!response.ok) {
            console.error(`API request failed with status ${response.status}:`, data);
            return res.status(response.status).json({ error: data });
        }

        const responseText = data.choices[0].message.content;
        await logInteractionToDb(characterId, details.name, interactions.join(' '), responseText);

        res.json({ response: responseText });
    } catch (error) {
        console.error('Error making API request:', error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

const logInteractionToDb = async (characterId, name, interaction, content) => {
    const timestamp = new Date().toISOString();
    console.log(`Logging interaction to database: ${characterId}, ${name}, ${interaction}, ${content}, ${timestamp}`);
    try {
        const result = await db.run(
            'INSERT INTO chat_log (CharacterId, Name, Interaction, Content, Timestamp) VALUES (?, ?, ?, ?, ?)',
            characterId, name, interaction, content, timestamp
        );
        console.log('Interaction logged to database', result);
    } catch (err) {
        console.error('Error logging interaction to database:', err);
    }
};

// New endpoint to fetch logs
app.get('/api/v1/chat/logs', async (req, res) => {
    try {
        const logs = await db.all('SELECT * FROM chat_log ORDER BY Timestamp DESC');
        res.json(logs);
    } catch (err) {
        console.error('Error fetching logs:', err);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

app.listen(PORT, () => {
    console.log(`Server is running on http://localhost:${PORT}`);
});
