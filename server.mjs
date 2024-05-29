import express from 'express';
import bodyParser from 'body-parser';
import fetch from 'node-fetch';
import Database from 'better-sqlite3';
import path from 'path';
import dotenv from 'dotenv';
import chalk from 'chalk';
import figlet from 'figlet';

dotenv.config();

const app = express();
const PORT = 3000;
const __dirname = path.dirname(new URL(import.meta.url).pathname).substring(1);
const DB_PATH = path.join(__dirname, 'chat_log.db');

app.use(bodyParser.json());

let db;

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

app.post('/api/v1/chat/completions', async (req, res) => {
    console.log(chalk.blue('Received request:'), req.body);

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
                role: 'user',
                content: prompt
            }
        ]
    };

    console.log(chalk.magenta('Request Body:'), requestBody);

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
        console.log(chalk.green('Response from API:'), data);

        if (!response.ok) {
            console.error(chalk.red(`API request failed with status ${response.status}:`), data);
            return res.status(response.status).json({ error: data });
        }

        const responseText = data.choices[0].message.content;
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

// New endpoint to fetch logs
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
    figlet.text('LIVING RIM', {
        font: 'Slant', // Change to your desired font
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
        console.log(chalk.hex('#FFA500')('Powered by rats!')); // Light orange color
        console.log(chalk.cyan(`Server is running on http://localhost:${PORT}`));
    });
});
