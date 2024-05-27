# LivingRim

A RimWorld mod that integrates a language model to enhance gameplay with dynamic character interactions.

## Features

- Dynamic character dialogues using language models.
- Multi-turn conversations with contextual recall.
- Support for multiple language model APIs (OpenRouter, OpenAI, Cohere).

## Planned Features

- Dynamic events using language models
- Support for local models
- Better overall integration

## Configuration

### Default Configuration

By default, the mod uses OpenRouter for language model interactions. You can find the configuration file at `Mods/LivingRim/api/config.json`.

```json
{
    "default_api": "openrouter",
    "apis": {
        "openrouter": {
            "api_key": "YOUR_OPENROUTER_API_KEY",
            "max_tokens": 150,
            "temperature": 0.7,
            "model": "gpt-4"
        },
        "openai": {
            "api_key": "YOUR_OPENAI_API_KEY",
            "max_tokens": 150,
            "temperature": 0.7,
            "model": "davinci-codex"
        },
        "cohere": {
            "api_key": "YOUR_COHERE_API_KEY",
            "max_tokens": 150,
            "temperature": 0.7,
            "model": "xlarge"
        }
    }
}
