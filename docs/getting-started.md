# Getting Started

## Structure

This app is composed of three main sections:

- Generator
  - Located in the `Generator` directory.
- Client
  - Located in the `packages/client` directory.
- Server
  - Located in the `packages/server` directory.

The Generator takes in settings and uses an algorithm to place items.
The Generator also handles creating GCI file data.
It is written in C#.

The Client is the client-side of the website.
It is written in JS, CSS, and HTML.

The Server is the backend of the website.
It is written in TypeScript.

## Requirements

For local development, you will need the following:

- Node (runs JS code)
- Yarn (JS package manager)
- dotNET (for Generator C# code)

If you need to interact with the deployment process, you will also need Docker.
After completing the rest of this document, see [docker.md](./docker.md) for further instructions.

### Node

Install the latest LTS version of [Node](https://nodejs.org/en/).

- Set `corepack manager` to "Entire feature will be unavailable" (do not install it).
- You can leave the C/C++ modules part unchecked.
- Complete the Node installation.

_Note: We leave corepack unchecked because it can cause problems, but if you already use it and have access to Node and Yarn, then you can skip these steps for Node and Yarn._

### Yarn

Install Yarn with the following command:

`npm install -g yarn`

Ignore any warnings about config global being deprecated.
We will never use npm again, so it is not important.

You should now be able to run `yarn` commands in command lines.<br>
_Note: You may need to open a new command line for `yarn` to be available._

### dotNET

The backend of the generator code runs on `dotnet` version `8.0` the SDK can be downloaded [here](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

## Setup

Run `yarn` in the root directory of the repository.
This will install all javascript dependencies.

## Development

### Generator

// TODO: Add better instructions for C# here

The Generator can be worked on by itself without going through the website.

If you need the Generator to be available to the server, you will need to build it after any changes.

Currently, I am doing this by putting a breakpoint near the start of the Program.cs file, then stopping the Generator once the breakpoint is hit.
There is probably a way to just build the C# code with a single command, but I don't know it at the moment.

Here is a crappy configuration you can use in VSCode for this:

```jsonc
{
  // Use IntelliSense to find out which attributes exist for C# debugging
  // Use hover for the description of the existing attributes
  // For further information visit https://github.com/OmniSharp/omnisharp-vscode/blob/master/debugger-launchjson.md
  "name": "generate2 w/args .NET Core Launch (console)",
  "type": "coreclr",
  "request": "launch",
  "preLaunchTask": "build",
  // If you have changed target frameworks, make sure to update the program path.
  "program": "${workspaceFolder}/Generator/bin/Debug/net5.0/TPRandomizer.dll",
  // "args": ["QVFZUUFBQUFBNzc3NEFBQUFBQUFBQUE=", "[0]-Attentive-Dangoro"],
  "args": [
    "generate2",
    "idnull",
    // "null",
    // "QVFZUUFBQUFBNzc3NFlBQUFBQUFBQUFBQUJSR09GNg==",
    // "0s0H13400000_7v89H5_u",
    // "0s0Q13400000_7v09H5__G807cc1_u",
    // "0sPN11700000_91A8V_Jm-Gaq_u",
    "0s9A3Fod_-V__W",
    "DYIteDjcACDaz7SnA76sRW"
  ],
  // "cwd": "${workspaceFolder}/Generator",
  "cwd": "${workspaceFolder}",
  // For more information about the 'console' field, see https://aka.ms/VSCode-CS-LaunchJson-Console
  "console": "internalConsole",
  "stopAtEntry": false
}
```

### Running the server

During development, the server is run by `nodemon`.

Nodemon will watch for changes to files, and it will restart the server after any changes to server files.

Its output will also display any TypeScript errors, and it will display output which we print to the console.

The recommended steps for running the server are:

- Open a new command line.
- From the root directory, run `yarn server:debug`.

This will start the server, and you will be able to attach to it to debug with breakpoints (more on this later).

The website will be accessible at `http://localhost:3500`.

Using VSCode, you can attach to the server with the following debug configuration:

```json
{
  "name": "Attach to server",
  "port": 9229,
  "request": "attach",
  "skipFiles": ["<node_internals>/**"],
  "type": "pwa-node",
  "restart": true
}
```

You can also start the server with command `yarn server:debug-b`.
This will start the server, but it will pause immediately and wait for you to connect a debugger before continuing.
This is handy if you need to put a breakpoint near the very start of execution.

### Editing client code

_Note: This section will change after the client-side code is restructured in an update coming soon._

Assuming you are just making changes to existing client code, you can make a change to the file, then reload the page in your browser.

## Development Environment

VSCode is the recommended tool for development.

### Formatting

#### Prettier

In order to match the formatting, you should install the Prettier extension (the one with over 30 million installs): https://marketplace.visualstudio.com/items?itemName=esbenp.prettier-vscode

- Prettier will format several file types for you, including HTML, JS, JSON, and Markdown.
- The `.prettierrc.json` file at the root of the repository defines the settings for Prettier.

#### CSharpier

For formatting CSharp code, you should install CSharpier: https://marketplace.visualstudio.com/items?itemName=csharpier.csharpier-vscode

It is based on Prettier, hence the name.

## Next steps

See [docker.md](./docker.md) for instructions on deploying a production build.
