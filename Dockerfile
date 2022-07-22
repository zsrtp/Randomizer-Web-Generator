# Client
FROM node:lts-alpine AS packages-build

WORKDIR /usr/app/

COPY ./package.json .
COPY ./yarn.lock .
COPY ./packages/client/package.json ./packages/client/package.json
COPY ./packages/server/package.json ./packages/server/package.json

RUN yarn

COPY ./packages/server/ ./packages/server/

WORKDIR /usr/app/packages/server
RUN yarn build
COPY .env ./dist/

WORKDIR /usr/app/
COPY ./packages/client/ ./packages/client

#####

FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine AS build
WORKDIR /usr/app/generatorSrc

# copy csproj and restore as distinct layers
COPY ./Generator .
RUN dotnet restore "./TPRandomizer.csproj" --disable-parallel

# copy and publish app and libraries
RUN dotnet publish "./TPRandomizer.csproj" -c release -o /app/generator --no-restore

# RUN mkdir /app/generator/Generator
RUN cp -r World /app/generator/World

FROM node:lts-alpine as node_base
FROM mcr.microsoft.com/dotnet/runtime:6.0-alpine
COPY --from=node_base . .
WORKDIR /app
COPY --from=build /app .
COPY .env ./generator
# ENTRYPOINT ["dotnet", "dotnetapp.dll"]

# CMD ["sleep", "infinity"]


COPY --from=packages-build /usr/app/packages/client /usr/app/client/build
COPY --from=packages-build /usr/app/packages/server/dist /usr/app/server

WORKDIR /usr/app/server

ENV TPRGEN_ENV=production
ENV NODE_ENV=production

EXPOSE 3500

ARG IMAGE_VERSION
ENV IMAGE_VERSION $IMAGE_VERSION

ARG GIT_COMMIT
ENV GIT_COMMIT $GIT_COMMIT

CMD ["node", "bundle.js"]
