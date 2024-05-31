import express from 'express';
import bodyParser from 'body-parser';
import fetch from 'node-fetch';
import Database from 'better-sqlite3';
import path from 'path';
import fs from 'fs';
import dotenv from 'dotenv';
import chalk from 'chalk';
import figlet from 'figlet';
import winston from 'winston';

dotenv.config();

const app = express();
const PORT = 3000;

// Decode the URL-encoded __dirname to handle spaces
const decodedDirname = decodeURIComponent(path.dirname(new URL(import.meta.url).pathname).substring(1));
const DB_DIR = path.join(decodedDirname, 'data');
const LOG_DIR = path.join(decodedDirname, 'logs');
const DB_PATH = path.join(DB_DIR, 'chat_log.db');

// Ensure the database and log directories exist
if (!fs.existsSync(DB_DIR)) {
    fs.mkdirSync(DB_DIR, { recursive: true });
    console.log(chalk.green(`Created database directory at ${DB_DIR}`));
}
if (!fs.existsSync(LOG_DIR)) {
    fs.mkdirSync(LOG_DIR, { recursive: true });
    console.log(chalk.green(`Created logs directory at ${LOG_DIR}`));
}

// Log the database path
console.log(chalk.green(`Database path: ${DB_PATH}`));

// Set up winston logger
const logger = winston.createLogger({
    level: 'info',
    format: winston.format.combine(
        winston.format.timestamp({
            format: 'YYYY-MM-DD HH:mm:ss'
        }),
        winston.format.printf(info => `${info.timestamp} ${info.level}: ${info.message}`)
    ),
    transports: [
        new winston.transports.File({ filename: path.join(LOG_DIR, 'server.log') }),
        new winston.transports.Console({ format: winston.format.simple() })
    ]
});

app.use(bodyParser.json());

let db;

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
        logger.info('Database initialized and chat_log table created');
    } catch (error) {
        logger.error('Failed to initialize database:', error);
    }
})();

// Function to log interactions to the database
const logInteractionToDb = (characterId, name, interaction, content) => {
    const timestamp = new Date().toISOString();
    try {
        const stmt = db.prepare('INSERT INTO chat_log (CharacterId, Name, Interaction, Content, Timestamp) VALUES (?, ?, ?, ?, ?)');
        stmt.run(characterId, name, interaction, content, timestamp);
        const chatLogEntry = { characterId, name, interaction, content, timestamp };
        logger.info(`Logged interaction to database: ${JSON.stringify(chatLogEntry)}`);
        return chatLogEntry;
    } catch (err) {
        logger.error('Error logging interaction to database:', err);
        return null;
    }
};

// Endpoint to handle chat completions
app.post('/api/v1/chat/completions', async (req, res) => {
    logger.info('Received request:', JSON.stringify(req.body));

    const { characterId, interactions, details } = req.body;

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
    ### Instruction:
    The following is a role-play conversation between you and the user. You are ${details.name}, a character in RimWorld with the following details:
    
    **Character Details:**
    - **Name:** ${details.name}
    - **Mood:** ${details.mood}
    - **Health:** ${details.health}
    - **Personality:** ${details.personality}
    - **Relationships:** ${details.relationships}
    - **Environment:** ${details.environment}
    - **Needs:** ${details.needs}
    - **Backstory:** ${details.backstory}
    - **Skills:** ${skills}
    - **Passions:** ${passions}
    - **Current Job:** ${details.currentJob}
    - **Inventory:** ${details.inventory}
    - **Recent Events:** ${recentEvents}
    
    ### Input:
    Interaction: ${interactions.join(' ')}
    
    ### Response:
    `;

    const apiKey = process.env.OPENROUTER_API_KEY;
    const model = 'gryphe/mythomax-l2-13b';
    const maxTokens = 150;
    const siteUrl = 'https://www.livingrim.com';
    const appName = 'LivingRim by Ratstopher';

    const requestBody = {
        model: model,
        messages: [
            {
                role: 'assistant',
                content: prompt,
                max_tokens: maxTokens
            }
        ]
    };

    try {
        const response = await fetch('https://openrouter.ai/api/v1/chat/completions', {
            method: 'POST',
            headers: {
                'Authorization': `Bearer ${apiKey}`,
                'Content-Type': 'application/json',
                'HTTP-Referer': siteUrl,
                'X-Title': appName
            },
            body: JSON.stringify(requestBody)
        });

        const data = await response.json();

        if (!response.ok) {
            logger.error(`API request failed with status ${response.status}:`, data);
            return res.status(response.status).json({ error: data });
        }

        const responseText = data.choices[0].message.content;
        logInteractionToDb(characterId, details.name, interactions.join(' '), responseText);

        res.json({ response: responseText });
    } catch (error) {
        logger.error('Error making API request:', error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

// Endpoint to get all logs
app.get('/api/v1/chat/logs', (req, res) => {
    try {
        const stmt = db.prepare('SELECT * FROM chat_log');
        const logs = stmt.all();
        res.json(logs);
    } catch (error) {
        logger.error('Error fetching logs:', error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

// Endpoint to get logs by character ID
app.get('/api/v1/chat/logs/:characterId', (req, res) => {
    const { characterId } = req.params;
    try {
        const stmt = db.prepare('SELECT * FROM chat_log WHERE CharacterId = ?');
        const logs = stmt.all(characterId);
        res.json(logs);
    } catch (error) {
        logger.error(`Error fetching logs for character ${characterId}:`, error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

// Endpoint to get logs by name
app.get('/api/v1/chat/logs/name/:name', (req, res) => {
    const { name } = req.params;
    try {
        const stmt = db.prepare('SELECT * FROM chat_log WHERE Name = ?');
        const logs = stmt.all(name);
        res.json(logs);
    } catch (error) {
        logger.error(`Error fetching logs for name ${name}:`, error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

// Endpoint to get logs by date range
app.get('/api/v1/chat/logs/date', (req, res) => {
    const { startDate, endDate } = req.query;
    try {
        const stmt = db.prepare('SELECT * FROM chat_log WHERE Timestamp BETWEEN ? AND ?');
        const logs = stmt.all(startDate, endDate);
        res.json(logs);
    } catch (error) {
        logger.error('Error fetching logs by date range:', error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

// Start the server
app.listen(PORT, () => {
    figlet.text('LivingRim', {
        font: 'Slant',
        horizontalLayout: 'default',
        verticalLayout: 'default',
        width: 80,
        whitespaceBreak: true
    }, (err, data) => {
        if (err) {
            logger.error('Error generating ASCII art:', err);
            return;
        }
        console.log(chalk.green(data));
        console.log(chalk.hex('#FFA500')('Powered by rats!'));
        logger.info(`Server is running on http://localhost:${PORT}`);
    });
});
