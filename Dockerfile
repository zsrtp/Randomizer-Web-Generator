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

FROM mcr.microsoft.com/dotnet/sdk:5.0-alpine AS build
WORKDIR /usr/app/generatorSrc

# copy csproj and restore as distinct layers
# COPY ./Generator/TPRandomizer.csproj .
# COPY ./Generator/Properties ./Properties
# COPY ./Generator/Assets ./Assets
COPY ./Generator .
# RUN dotnet restore
RUN dotnet restore "./TPRandomizer.csproj" --disable-parallel

# copy and publish app and libraries
# COPY ./Generator .
# RUN dotnet publish -c release -o /app --no-restore
RUN dotnet publish "./TPRandomizer.csproj" -c release -o /app/generator --no-restore

# COPY ./Generator .
# RUN dotnet restore "./TPRandomizer.csproj" --disable-parallel
# RUN dotnet publish "./TPRandomizer.csproj" -c release -o output --no-restore

RUN mkdir /app/generator/Generator
RUN cp -r World /app/generator/Generator/World

FROM node:lts-alpine as node_base
FROM mcr.microsoft.com/dotnet/runtime:5.0-alpine
COPY --from=node_base . .
WORKDIR /app
COPY --from=build /app .
# ENTRYPOINT ["dotnet", "dotnetapp.dll"]

CMD ["sleep", "infinity"]
