import express from 'express';
import bodyParser from 'body-parser';
import fetch from 'node-fetch';
import fs from 'fs';
import path from 'path';
import dotenv from 'dotenv';

dotenv.config();

const app = express();
const PORT = 3000;
const __dirname = path.dirname(new URL(import.meta.url).pathname).substring(1);
const LOG_FILE_PATH = path.join(__dirname, 'chat_log.txt');

app.use(bodyParser.json());

app.post('/api/v1/chat/completions', async (req, res) => {
    console.log('Received request:', req.body);

    // Extract data from request
    const { characterId, interactions, details } = req.body;

    const prompt = `
    Name: ${details.name}
    Mood: ${details.mood}
    Health: ${details.health}
    Personality: ${details.personality}
    Relationships: ${details.relationships}

    The following is a conversation between You and user, You are ${details.name}, a character in RimWorld. ${details.name} has the personality traits of ${details.personality} and their current mood is ${details.mood}. They have the following relationships: ${details.relationships}.
    Interaction: ${interactions.join(' ')}
    `;

    // Use the API key from environment variable
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
        logInteraction(characterId, interactions.join(' '), responseText);

        res.json({ response: responseText });
    } catch (error) {
        console.error('Error making API request:', error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

const logInteraction = (characterId, interaction, response) => {
    const timestamp = new Date().toISOString();
    const logEntry = `${timestamp} Player: ${interaction}\n${timestamp} ${characterId}: ${response}\n`;
    fs.appendFileSync(LOG_FILE_PATH, logEntry, 'utf8');
};

app.listen(PORT, () => {
    console.log(`Server is running on http://localhost:${PORT}`);
});
