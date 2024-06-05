# LivingRim

## Overview
LivingRim is a mod for RimWorld that enhances character interactions by integrating advanced AI capabilities. This mod uses a custom server to process and generate responses, making your RimWorld experience more immersive and engaging.

## Features
- Enhanced dialog with AI-generated responses.
- Detailed character profiles and interactions.
- Customizable character personas and descriptions.
- Integration with an external AI model for realistic conversations.

## Disclaimer
This is an early build of LivingRim and is prone to issues. Please report any bugs or problems you encounter.

## Requirements
- RimWorld (any version that supports the required mods).
- The following RimWorld mods:
  - [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)
- Node.js (for running the custom server).
- Cohere API key.

## Installation

### Step 1: Download and Install Required Mods
1. Install the required RimWorld mods:
   - [Harmony](https://steamcommunity.com/sharedfiles/filedetails/?id=2009463077)

### Step 2: Obtain a Cohere API Key
1. Visit the [Cohere website](https://cohere.com/).
2. Sign up for a free trial account.
3. Once you have created an account, navigate to the API Keys section in your Cohere dashboard.
4. Generate a new API key and copy it. This will be used in the next steps.

### Step 3: Setup the Server

#### Windows
1. Clone or download the LivingRim repository.
2. Open a terminal and navigate to the `LivingRim` directory.
3. Run the `install_server.bat` script to install the required Node.js packages:
   ```sh
   install_server.bat
   ```
4. Create a `.env` file in the `LivingRim/Servers` directory with the following content:
   ```plaintext
   COHERE_API_KEY=your_cohere_api_key_here
   PORT=3000
   ```
   Replace `your_cohere_api_key_here` with your actual Cohere API key.

5. Start the server by running:
   ```sh
   start_server.bat
   ```

#### Linux
1. Clone or download the LivingRim repository.
2. Open a terminal and navigate to the `LivingRim/Servers` directory.
3. Run the following command to install the required Node.js packages:
   ```sh
   npm install
   ```
4. Create a `.env` file in the `LivingRim/Servers` directory with the following content:
   ```plaintext
   COHERE_API_KEY=your_cohere_api_key_here
   PORT=3000
   ```
   Replace `your_cohere_api_key_here` with your actual Cohere API key.

5. Start the server by running:
   ```sh
   npm start
   ```

### Step 4: Install the Mod in RimWorld

1. Move the `LivingRim` folder to your RimWorld Mods directory. The path typically looks like this:
   - **Windows**: `D:\Games\Steam\steamapps\common\RimWorld\Mods`
   - **Linux**: `/path/to/your/RimWorld/Mods` (replace with your actual RimWorld installation path)

2. Launch RimWorld and enable the LivingRim mod along with the required mods from the mod menu.

### Step 5: Configure and Play
1. Start a new game or load an existing game.
2. Interact with characters to experience enhanced dialog and interactions.

## Usage

### Customizing Character Personas
You can customize the personas and descriptions of characters using the mod's in-game interface. The data is saved and loaded automatically.

### Server Logs
The server logs interactions and errors to `logs/server.log`. Check this file if you encounter issues.

### Saving and Loading Data
The mod saves data in the `data` folder within the `Servers` directory. Ensure this folder is writable by the game and server.

## Troubleshooting
- **Mod Not Loading**: Ensure all required mods are installed and enabled.
- **Server Errors**: Check the server logs for detailed error messages.
- **Missing Data**: Ensure the `data` folder exists and is writable.

## Contribution
Feel free to fork this repository, submit issues, and contribute to the development of this mod.

## Contact

Please post any issues here to Github, or contact me at: ratstopher@proton.me

## License
This project is licensed under the MIT License.

---

**Enjoy a more immersive RimWorld experience with LivingRim!**
```

### install_server.bat
```bat
@echo off
echo Installing server dependencies...
cd Servers
npm install
echo Installation complete.
pause
```

### start_server.bat
```bat
@echo off
echo Starting Node.js server...
cd Servers
npm start
pause
