import express from 'express';
import bodyParser from 'body-parser';
import fetch from 'node-fetch';
import dotenv from 'dotenv';

dotenv.config();

const app = express();
const PORT = 3000;

app.use(bodyParser.json());

app.post('/api/v1/chat/completions', async (req, res) => {
    console.log('Received request:', req.body);

    // Extract data from request
    const { characterId, interactions } = req.body;

    // Use the API key from environment variable
    const apiKey = process.env.OPENROUTER_API_KEY;
    console.log('Using API Key:', apiKey);  // Add this line for debugging

    const requestBody = {
        model: 'openai/gpt-3.5-turbo',
        messages: interactions.map(content => ({ role: 'user', content }))
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

        res.json({ response: data.choices[0].text });
    } catch (error) {
        console.error('Error making API request:', error);
        res.status(500).json({ error: 'Internal Server Error' });
    }
});

app.listen(PORT, () => {
    console.log(`Server is running on http://localhost:${PORT}`);
});
