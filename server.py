from flask import Flask, request, jsonify
import os
import openai

app = Flask(__name__)

openai.api_key = os.getenv("OPENROUTER_API_KEY")
openai.api_base = "https://openrouter.ai/api/v1"

@app.route('/get_response', methods=['POST'])
def get_response():
    data = request.json
    prompt = data.get('prompt')
    character_id = data.get('characterId')

    if not prompt:
        return jsonify({'error': 'Prompt is required'}), 400

    try:
        response = openai.Completion.create(
            model="openai/gpt-3.5-turbo",  # Replace with the desired model
            prompt=prompt,
            max_tokens=1500,
            temperature=0.7
        )
        response_text = response.choices[0].text.strip()
    except Exception as e:
        return jsonify({'error': str(e)}), 500

    return jsonify({'response': response_text})

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=5000)
