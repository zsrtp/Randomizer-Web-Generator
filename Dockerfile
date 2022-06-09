# # FROM node:14 AS ui-build
# FROM node:lts-alpine AS ui-build

# WORKDIR /usr/app/client/
# COPY package.json .
# COPY yarn.lock .
# RUN yarn
# COPY src ./src
# COPY public ./public
# RUN yarn build

# # FROM node:14 AS server-build
# FROM node:lts-alpine AS server-build

# WORKDIR /usr/app/

# COPY --from=ui-build /usr/app/client/build ./client/build
# WORKDIR /usr/app/server/

# # COPY package*.json .
# # RUN npm install
# COPY package.json .
# COPY yarn.lock .
# RUN yarn

# COPY server.js .

# ENV NODE_ENV=production

# EXPOSE 5000

# CMD ["node", "server.js"]

#####

# Client
FROM node:lts-alpine AS ui-build

WORKDIR /usr/app/client/
# COPY package.json .
# COPY yarn.lock .
# RUN yarn
# COPY src ./src
# COPY public ./public
# RUN yarn build
COPY ./packages/client/ ./build/

# Server
FROM node:lts-alpine AS server-build
WORKDIR /usr/app/server/

COPY package.json .
COPY yarn.lock .
RUN yarn

COPY .env .
COPY server.js .
COPY util.js .

# ENV NODE_ENV=production

# EXPOSE 5000

# CMD ["node", "server.js"]

#####

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /usr/app/generatorSrc

# copy csproj and restore as distinct layers
COPY ./Generator .
RUN dotnet restore "./TPRandomizer.csproj" --disable-parallel

# copy and publish app and libraries
RUN dotnet publish "./TPRandomizer.csproj" -c release -o /app/generator --no-restore

# RUN mkdir /app/generator/Generator
RUN cp -r World /app/generator/World

FROM node:lts-alpine as node_base
FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine
COPY --from=node_base . .
WORKDIR /app
COPY --from=build /app .
COPY .env ./generator
# ENTRYPOINT ["dotnet", "dotnetapp.dll"]

# CMD ["sleep", "infinity"]


COPY --from=ui-build /usr/app/client/build /usr/app/client/build
COPY --from=server-build /usr/app/server /usr/app/server

WORKDIR /usr/app/server

ENV TPR_ENV=production
ENV NODE_ENV=production

EXPOSE 5000

CMD ["node", "server.js"]
