# LivingRim

LivingRim is a RimWorld mod that integrates with an external API to provide enhanced character interactions. This project consists of a RimWorld mod written in C# and a Node.js server that handles API interactions.

## Features

- Enhanced character interactions using an external AI model.
- Simple server setup for handling API requests.
- Modular code structure for easy maintenance and extension.

## Prerequisites

- [RimWorld](https://rimworldgame.com/) installed.
- [Node.js](https://nodejs.org/) installed.
- An API key for the external AI service (e.g., OpenRouter).

## Installation

### Mod Installation

1. Clone this repository to your RimWorld `Mods` directory:
    ```sh
    git clone https://github.com/yourusername/LivingRim.git
    ```

2. Open RimWorld and enable the `LivingRim` mod in the mod settings.

### Server Setup

1. Navigate to the `LivingRim` directory:
    ```sh
    cd LivingRim
    ```

2. Install the Node.js dependencies:
    ```sh
    npm install
    ```

3. Create a `.env` file in the `LivingRim` directory with your API key:
    ```
    OPENROUTER_API_KEY=your_api_key_here
    ```

4. Start the server:
    ```sh
    node server.mjs
    ```

## Usage

1. Ensure the Node.js server is running:
    ```sh
    node server.mjs
    ```

2. Start RimWorld and load your game.
3. Interact with characters in-game. The mod will send interaction data to the Node.js server, which will handle the API request and return the response to the game.

## Configuration

The server configuration is handled via environment variables. Create a `.env` file in the root directory of the project and add your API key:
```
OPENROUTER_API_KEY=your_api_key_here
```

## Development

### Mod Development

- The mod source code is located in the `Source` directory.
- Use an IDE like Visual Studio or JetBrains Rider to work on the C# code.
- Build the mod using your IDE's build tools.

### Server Development

- The server code is located in `server.mjs`.
- Use an editor like VSCode for editing the server code.
- Run `npm start` to start the server for development.

## Contributing

Contributions are welcome! Please fork this repository and submit pull requests.

## License

This project is licensed under the MIT License. See the `LICENSE` file for details.



