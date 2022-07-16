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
It is written in JS, CSS, and HTML (though it is expected to swap to React in an update fairly soon).

The Server is the backend of the website.
It is written in TypeScript.

## Requirements

For local development, you will need the following:

- Node (runs JS code)
- Yarn (JS package manager)
- dotNET (for Generator C# code)

If you need to interact with the deployment process, you will also need Docker.
After completing the rest of this document, see [deployment.md](./deployment.md) for further instructions.

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

// TODO: Add instructions here after swapping to 6.0

## Setup

Run `yarn` in the root directory of the repository.
This will install all javascript dependencies.

## Development

// TODO: Add instructions for C# here
