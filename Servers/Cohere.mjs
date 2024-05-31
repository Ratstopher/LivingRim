import express from 'express';
import bodyParser from 'body-parser';
import fetch from 'node-fetch';
import Database from 'better-sqlite3';
import path from 'path';
import fs from 'fs';
import dotenv from 'dotenv';
import chalk from 'chalk';
import figlet from 'figlet';

dotenv.config();

const app = express();
const PORT = 3000;

// Decode the URL-encoded __dirname to handle spaces
const decodedDirname = decodeURIComponent(path.dirname(new URL(import.meta.url).pathname).substring(1));
const DB_DIR = path.join(decodedDirname, 'data');
const DB_PATH = path.join(DB_DIR, 'chat_log.db');

app.use(bodyParser.json());

let db;

// Ensure the database directory exists
if (!fs.existsSync(DB_DIR)) {
    fs.mkdirSync(DB_DIR, { recursive: true });
    console.log(chalk.green(`Created database directory at ${DB_DIR}`));
}

// Log the database path
console.log(chalk.green(`Database path: ${DB_PATH}`));

// Initialize database and create table if it doesn't exist
(async () => {
    try {
        db = new Database(DB_PATH);
        db.exec(`
            CREATE TABLE IF NOT EXISTS chat_log (
                id INTEGER PRIMARY KEY AUTOINCREMENT,
                CharacterId TEXT,
                Name TEXT,
                Interaction TEXT,
                Content TEXT,
                Timestamp TEXT
            )
        `);
        console.log(chalk.green('Database initialized and chat_log table created'));
    } catch (error) {
        console.error(chalk.red('Failed to initialize database:', error));
    }
})();

// Endpoint to handle chat completions
app.post('/api/v1/chat/completions', async (req, res) => {
    console.log(chalk.blue('Received request:'), req.body);

    const { characterId, interactions, details } = req.body;
    const MESSAGE_LIMIT = 10;  // Limit the number of messages to append

    // Fetch conversation history from the database
    const previousInteractions = db.prepare('SELECT Interaction, Content FROM chat_log WHERE CharacterId = ? ORDER BY Timestamp DESC LIMIT ?').all(characterId, MESSAGE_LIMIT);

    // Construct the conversation history
    let conversationHistory = '';
    for (const interaction of previousInteractions.reverse()) {
        conversationHistory += `User: ${interaction.Interaction}\n${details.name}: ${interaction.Content}\n`;
    }

    // Append the new interactions
    for (const interaction of interactions) {
        conversationHistory += `User: ${interaction}\n${details.name}: `;
    }

    const skills = details.skills
        ? Object.entries(details.skills).map(([skill, level]) => `${skill}: ${level}`).join(', ')
        : 'No skills available';

    const passions = details.passions
        ? Object.entries(details.passions).map(([skill, passion]) => `${skill}: ${passion}`).join(', ')
        : 'No passions available';

    const recentEvents = details.recentEvents
        ? details.recentEvents.join(', ')
        : 'No recent events available';

    const prompt = `
        Character Details:
        Name: ${details.name}
        Mood: ${details.mood}
        Health: ${details.health}
        Personality: ${details.personality}
        Relationships: ${details.relationships}
        Environment: ${details.environment}
        Needs: ${details.needs}
        Backstory: ${details.backstory}
        Skills: ${skills}
        Passions: ${passions}
        Current Job: ${details.currentJob}
        Inventory: ${details.inventory}
        Recent Events: ${recentEvents}

        Role-play the following conversation as ${details.name}, a character in RimWorld with the above attributes. The conversation so far is:
    ${conversationHistory}`;


    const apiKey = process.env.COHERE_API_KEY;
    const model = process.env.COHERE_MODEL;

    const requestBody = {
        model: model,
        prompt: prompt,
        max_tokens: 300,
        temperature: 0.7,
        k: 0,
        p: 1,
        frequency_penalty: 0,
        presence_penalty: 0,
        stop_sequences: []
    };

    console.log(chalk.magenta('Request Body:'), requestBody);

    try {
        const response = await fetch('https://api.cohere.ai/v1/generate', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${apiKey}`,
                'Content-Type': 'application/json'
            },
            body: JSON.stringify(requestBody)
        });

        const data = await response.json();
        console.log(chalk.green('Response from API:'), data);

        if (!response.ok) {
            console.error(chalk.red(`API request failed with status ${response.status}:`), data);
            return res.status(response.status).json({ error: data });
        }

        const responseText = data.generations[0].text;
        logInteractionToDb(characterId, details.name, interactions.join(' '), responseText);

        res.json({ response: responseText });
    } catch (error) {
        console.error(chalk.red('Error making API request:', error));
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

/**
 * Logs the interaction to the database.
 * @param {string} characterId - The ID of the character.
 * @param {string} name - The name of the character.
 * @param {string} interaction - The player's interaction message.
 * @param {string} content - The character's response message.
 */
const logInteractionToDb = (characterId, name, interaction, content) => {
    const timestamp = new Date().toISOString();
    console.log(chalk.yellow(`Logging interaction to database: ${characterId}, ${name}, ${interaction}, ${content}, ${timestamp}`));
    try {
        const stmt = db.prepare('INSERT INTO chat_log (CharacterId, Name, Interaction, Content, Timestamp) VALUES (?, ?, ?, ?, ?)');
        const result = stmt.run(characterId, name, interaction, content, timestamp);
        console.log(chalk.green('Interaction logged to database'), result);
    } catch (err) {
        console.error(chalk.red('Error logging interaction to database:', err));
    }
};

// Endpoint to fetch chat logs
app.get('/api/v1/chat/logs', async (req, res) => {
    try {
        const logs = db.prepare('SELECT * FROM chat_log ORDER BY Timestamp DESC').all();
        res.json(logs);
    } catch (err) {
        console.error(chalk.red('Error fetching logs:', err));
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

app.listen(PORT, () => {
    figlet.text('LIVINGRIM', {
        font: 'Alligator',
        horizontalLayout: 'default',
        verticalLayout: 'default',
        width: 80,
        whitespaceBreak: true
    }, (err, data) => {
        if (err) {
            console.error('Error generating ASCII art:', err);
            return;
        }
        console.log(chalk.green(data));
        console.log(chalk.hex('#FFA500')('new coomander r+!'));
        console.log(chalk.cyan(`Server is running on http://localhost:${PORT}`));
    });
});
