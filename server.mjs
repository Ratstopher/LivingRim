import express from 'express';
import bodyParser from 'body-parser';
import fetch from 'node-fetch';
import dotenv from 'dotenv';

dotenv.config();

const app = express();
const PORT = 3000;
const logFilePath = 'chat_log.txt';

app.use(bodyParser.json());

app.post('/api/v1/chat/completions', async (req, res) => {
    console.log('Received request:', req.body);

    // Extract data from request
    const { characterId, interactions, details } = req.body;

    // Use the API key from environment variable
    const apiKey = process.env.OPENROUTER_API_KEY;

    // Construct a detailed prompt including character details
    const prompt = `
    Name: ${details.name}
    Mood: ${details.mood}
    Health: ${details.health}
    Personality: ${details.personality}
    Relationships: ${details.relationships}

    The following is a conversation with ${details.name}, a character in RimWorld. ${details.name} has the personality traits of ${details.personality} and their current mood is ${details.mood}. They have the following relationships: ${details.relationships}.
    Interaction: ${interactions.join(' ')}
    `;

    const requestBody = {
        model: 'mistralai/mistral-7b-instruct:free',
        messages: [{ role: 'user', content: prompt }]
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

        // Log the conversation to a file
        const logEntry = `Character ID: ${characterId}\nInteractions: ${interactions.join(', ')}\nResponse: ${data.choices[0].message.content}\n\n`;
        fs.appendFileSync(logFilePath, logEntry);
        
        if (!response.ok) {
            console.error(`API request failed with status ${response.status}:`, data);
            return res.status(response.status).json({ error: data });
        }

        res.json({ response: data.choices[0].message.content });
    } catch (error) {
        console.error('Error making API request:', error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

app.listen(PORT, () => {
    console.log(`Server is running on http://localhost:${PORT}`);
});
