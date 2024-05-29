Sure, here's the updated `README.md` with the requested changes:

```markdown
# LivingRim

LivingRim is a cutting-edge mod for RimWorld, designed to elevate character interactions by integrating with an advanced external API. This project is composed of a RimWorld mod written in C# and a Node.js server that facilitates API communications.

## Features

- **Enhanced Character Interactions**: Utilize an external AI model to create more dynamic and engaging character interactions.
- **Easy Server Setup**: Streamlined Node.js server for managing API requests seamlessly.
- **Efficient Prompt Structuring**: Gather and send comprehensive pawn information, including mood, health, personality, relationships, environment, needs, and backstory, to the AI model.

## Prerequisites

- **RimWorld**: Ensure you have RimWorld installed from [RimWorld](https://rimworldgame.com/).
- **Node.js**: Install the latest version from [Node.js](https://nodejs.org/).
- **API Key**: Obtain an API key from the external AI service you are using (e.g., OpenRouter).

## Installation

### Step 1: Clone the Repository

Clone this repository to your local machine:

```bash
git clone https://github.com/Ratstopher/LivingRim.git
cd LivingRim
```

### Step 2: Install Dependencies

Install the necessary npm packages by running the `install_server.bat` script:

```bash
install_server.bat
```

Alternatively, you can run the following command manually:

```bash
npm install
```

### Step 3: Set Up Environment Variables

Create a `.env` file in the project root directory and add your OpenRouter API key:

```plaintext
OPENROUTER_API_KEY=your-api-key-here
```

### Step 4: Start the Server

Start the server using the `start_server.bat` script:

```bash
start_server.bat
```

Alternatively, you can run the following command manually:

```bash
node server.mjs
```

## Usage

### Interacting with Pawns

Once the server is running, you can interact with pawns in-game through the new chat interface. To initiate a conversation, select a pawn and click the "Talk" button. Type your message and receive dynamic responses based on the pawn's context.

### Pawn Information

The mod gathers and sends comprehensive information about pawns to the AI model, including:
- **Mood**
- **Health**
- **Personality**
- **Relationships**
- **Environment**
- **Needs**
- **Backstory**

## Development

### Project Structure

- **server.mjs**: Main server file that handles API requests and database operations.
- **CharacterContext.cs**: Manages character context, interactions, and logs.
- **Dialog_Input.cs**: Handles the chat interface for user input and responses.
- **Dialog_ConversationDetails.cs**: Displays detailed conversation logs.
- **MainTabWindow_ChatLog.cs**: Main window for viewing chat logs.
- **LLMservice.cs**: Handles communication with the language model API.
- **Main.cs**: Initializes the mod using Harmony patches.
- **CharacterDetails.cs**: Defines the structure for character details.

### Running Locally

To run the server locally, follow the installation steps and use the provided batch scripts. Ensure you have the required environment variables set up.

### Planned Features

- **UI Improvements**: Enhancing the user interface for better interaction.
- **Mod Support Testing**: Ensuring compatibility with other mods.
- **More API Support**: Including local and horde LLM integration such as KoboldCPP, Oobabooga, and Kobold Horde.
- **Automated LLM Prompting**: Triggering LLM prompts based on character events.
- **LLM Simulated Conversations**: Facilitating conversations between two pawns using the LLM.
- **ChromaDB Support**: Enhancing contextual memory with ChromaDB.
- **More Prompt Categories**: Pulling additional prompt categories from pawns for richer interactions.

### Contributing

We welcome contributions to the LivingRim project! To contribute, please follow these steps:

1. Fork the repository.
2. Create a new branch (`git checkout -b feature-branch`).
3. Make your changes.
4. Commit your changes (`git commit -m 'Add some feature'`).
5. Push to the branch (`git push origin feature-branch`).
6. Open a Pull Request.

## Troubleshooting

If you encounter issues, please check the following:

- Ensure all dependencies are installed correctly.
- Verify that your environment variables are set up properly.
- Check the console output for error messages and stack traces.

## License

This project is licensed under the MIT License. See the [LICENSE](LICENSE) file for details.

## Acknowledgements

- [RimWorld](https://rimworldgame.com/) by Ludeon Studios
- [Harmony](https://harmony.pardeike.net/) by Andreas Pardeike
- [Node.js](https://nodejs.org/)
- [OpenRouter](https://openrouter.ai/)

## Contact

For questions or support, please open an issue on our [GitHub repository](https://github.com/Ratstopher/LivingRim/issues) or join our development Discord server (coming soon).

