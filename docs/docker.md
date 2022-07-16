# Docker

Docker provides a containerized environment for the website.
It packages up the code and all of its dependencies so that the website can run quickly and reliably on any machine you put it on.

To provide a little more detail:

- We will install docker on the machine which should host the website and turn on swarm mode (only need to do this once).
- We will build a docker **image** for a version of the website.
  - The image defines everything that is needed to run that version of the website, including things such as "we need version X.Y of this dependency".
- We will run a command to deploy that version of the website.

To deploy a new version, we simply build a new image and run the same command to automatically replace the old version.

## Setup

Visit [https://www.docker.com/get-started/](https://www.docker.com/get-started/).

If you are on a Windows machine, you will need to install **Docker Desktop**.

In a command line, enable swarm mode with `docker swarm init` on Windows.<br>
Other OSs should see [https://docs.docker.com/engine/swarm/swarm-mode/](https://docs.docker.com/engine/swarm/swarm-mode/) for more detailed instructions.

## Building the image

Make sure you have installed dependencies by running `yarn` in the root directory.

Run `yarn build` to build the image.

## Deploying the image

### Secret

Before deploying the image, you need to define a secret which is used in the production environment.

- Create a `secrets` directory at the root of the repository.
- In this directory, create a `jwtSecret.txt` file.
- It is recommended that you set the contents of this file to a random 32 character hex hash.
  - You can go to [this website](https://onlinehashtools.com/generate-random-md5-hash) and copy one of the hash lines if you would like.

### Deploying

Run `yarn deploy` to deploy the image.

Wait a few seconds, then you should be able to access the website at http://localhost:3000.

## Stop deploying

To stop deploying the website, you can run `yarn down`.
