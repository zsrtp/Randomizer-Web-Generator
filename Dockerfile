# Client
FROM node:lts-alpine AS packages-build

WORKDIR /usr/app/

RUN mkdir -p ./.yarn/releases
COPY ./.yarn/releases/yarn-3.2.1.cjs ./.yarn/releases/yarn-3.2.1.cjs
COPY ./.yarnrc.yml .
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

FROM mcr.microsoft.com/dotnet/sdk:8.0-alpine AS build
WORKDIR /usr/app/generatorSrc

# copy csproj and restore as distinct layers
COPY ./Generator .
RUN dotnet restore "./TPRandomizer.csproj" --disable-parallel

# copy and publish app and libraries
RUN dotnet publish "./TPRandomizer.csproj" -c release -o /app/generator --no-restore

# RUN mkdir /app/generator/Generator
RUN cp -R World /app/generator/World
RUN cp -R Translations /app/generator/Translations
RUN mkdir -p /app/generator/Assets/Sound
RUN cp -R Assets/Sound/BackgroundMusic.jsonc /app/generator/Assets/Sound/BackgroundMusic.jsonc
RUN mkdir -p /app/generator/Assets/Entrances
RUN cp -R Assets/Entrances/EntranceTable.jsonc /app/generator/Assets/Entrances/EntranceTable.jsonc
RUN mkdir -p /app/generator/Assets/CheckMetadata
RUN cp -R Assets/CheckMetadata/Gamecube /app/generator/Assets/CheckMetadata
RUN cp -R Assets/CheckMetadata/Wii1.0 /app/generator/Assets/CheckMetadata
RUN cp -R Assets/HintDistributions /app/generator/Assets/HintDistributions
RUN cp -R Assets/gci /app/generator/Assets/gci
RUN cp -R Assets/bootloader /app/generator/Assets/bootloader
RUN cp -R Assets/patch /app/generator/Assets/patch
RUN cp -R Assets/rels /app/generator/Assets/rels

FROM mcr.microsoft.com/dotnet/runtime:8.0-alpine

################################################################################
# Partial copy from
# https://github.com/nodejs/docker-node/blob/main/22/alpine3.21/Dockerfile in
# order to install Node so it is available in the image. This Node version
# matches what was used in node:lts-alpine at the time of this change
# (2024/12/10).
################################################################################
ENV NODE_VERSION 22.12.0

RUN addgroup -g 1000 node \
	&& adduser -u 1000 -G node -s /bin/sh -D node \
	&& apk add --no-cache \
	libstdc++ \
	&& apk add --no-cache --virtual .build-deps \
	curl \
	&& ARCH= OPENSSL_ARCH='linux*' && alpineArch="$(apk --print-arch)" \
	&& case "${alpineArch##*-}" in \
	x86_64) ARCH='x64' CHECKSUM="43532120bad06cdea17c2ffba81ebfcff4611532a3569ef996faa39aadcbc759" OPENSSL_ARCH=linux-x86_64;; \
	x86) OPENSSL_ARCH=linux-elf;; \
	aarch64) OPENSSL_ARCH=linux-aarch64;; \
	arm*) OPENSSL_ARCH=linux-armv4;; \
	ppc64le) OPENSSL_ARCH=linux-ppc64le;; \
	s390x) OPENSSL_ARCH=linux-s390x;; \
	*) ;; \
	esac \
	&& if [ -n "${CHECKSUM}" ]; then \
	set -eu; \
	curl -fsSLO --compressed "https://unofficial-builds.nodejs.org/download/release/v$NODE_VERSION/node-v$NODE_VERSION-linux-$ARCH-musl.tar.xz"; \
	echo "$CHECKSUM  node-v$NODE_VERSION-linux-$ARCH-musl.tar.xz" | sha256sum -c - \
	&& tar -xJf "node-v$NODE_VERSION-linux-$ARCH-musl.tar.xz" -C /usr/local --strip-components=1 --no-same-owner \
	&& ln -s /usr/local/bin/node /usr/local/bin/nodejs; \
	else \
	echo "Building from source" \
	# backup build
	&& apk add --no-cache --virtual .build-deps-full \
	binutils-gold \
	g++ \
	gcc \
	gnupg \
	libgcc \
	linux-headers \
	make \
	python3 \
	py-setuptools \
	# use pre-existing gpg directory, see https://github.com/nodejs/docker-node/pull/1895#issuecomment-1550389150
	&& export GNUPGHOME="$(mktemp -d)" \
	# gpg keys listed at https://github.com/nodejs/node#release-keys
	&& for key in \
	C0D6248439F1D5604AAFFB4021D900FFDB233756 \
	DD792F5973C6DE52C432CBDAC77ABFA00DDBF2B7 \
	CC68F5A3106FF448322E48ED27F5E38D5B0A215F \
	8FCCA13FEF1D0C2E91008E09770F7A9A5AE15600 \
	890C08DB8579162FEE0DF9DB8BEAB4DFCF555EF4 \
	C82FA3AE1CBEDC6BE46B9360C43CEC45C17AB93C \
	108F52B48DB57BB0CC439B2997B01419BD92F80A \
	A363A499291CBBC940DD62E41F10027AF002F8B0 \
	; do \
	gpg --batch --keyserver hkps://keys.openpgp.org --recv-keys "$key" || \
	gpg --batch --keyserver keyserver.ubuntu.com --recv-keys "$key" ; \
	done \
	&& curl -fsSLO --compressed "https://nodejs.org/dist/v$NODE_VERSION/node-v$NODE_VERSION.tar.xz" \
	&& curl -fsSLO --compressed "https://nodejs.org/dist/v$NODE_VERSION/SHASUMS256.txt.asc" \
	&& gpg --batch --decrypt --output SHASUMS256.txt SHASUMS256.txt.asc \
	&& gpgconf --kill all \
	&& rm -rf "$GNUPGHOME" \
	&& grep " node-v$NODE_VERSION.tar.xz\$" SHASUMS256.txt | sha256sum -c - \
	&& tar -xf "node-v$NODE_VERSION.tar.xz" \
	&& cd "node-v$NODE_VERSION" \
	&& ./configure \
	&& make -j$(getconf _NPROCESSORS_ONLN) V= \
	&& make install \
	&& apk del .build-deps-full \
	&& cd .. \
	&& rm -Rf "node-v$NODE_VERSION" \
	&& rm "node-v$NODE_VERSION.tar.xz" SHASUMS256.txt.asc SHASUMS256.txt; \
	fi \
	&& rm -f "node-v$NODE_VERSION-linux-$ARCH-musl.tar.xz" \
	# Remove unused OpenSSL headers to save ~34MB. See this NodeJS issue: https://github.com/nodejs/node/issues/46451
	&& find /usr/local/include/node/openssl/archs -mindepth 1 -maxdepth 1 ! -name "$OPENSSL_ARCH" -exec rm -rf {} \; \
	&& apk del .build-deps \
	# smoke tests
	&& node --version \
	&& npm --version

ENV YARN_VERSION 1.22.22

RUN apk add --no-cache --virtual .build-deps-yarn curl gnupg tar \
	# use pre-existing gpg directory, see https://github.com/nodejs/docker-node/pull/1895#issuecomment-1550389150
	&& export GNUPGHOME="$(mktemp -d)" \
	&& for key in \
	6A010C5166006599AA17F08146C2130DFD2497F5 \
	; do \
	gpg --batch --keyserver hkps://keys.openpgp.org --recv-keys "$key" || \
	gpg --batch --keyserver keyserver.ubuntu.com --recv-keys "$key" ; \
	done \
	&& curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz" \
	&& curl -fsSLO --compressed "https://yarnpkg.com/downloads/$YARN_VERSION/yarn-v$YARN_VERSION.tar.gz.asc" \
	&& gpg --batch --verify yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
	&& gpgconf --kill all \
	&& rm -rf "$GNUPGHOME" \
	&& mkdir -p /opt \
	&& tar -xzf yarn-v$YARN_VERSION.tar.gz -C /opt/ \
	&& ln -s /opt/yarn-v$YARN_VERSION/bin/yarn /usr/local/bin/yarn \
	&& ln -s /opt/yarn-v$YARN_VERSION/bin/yarnpkg /usr/local/bin/yarnpkg \
	&& rm yarn-v$YARN_VERSION.tar.gz.asc yarn-v$YARN_VERSION.tar.gz \
	&& apk del .build-deps-yarn \
	# smoke test
	&& yarn --version \
	&& rm -rf /tmp/*
################################################################################
# End copied lines to install Node
################################################################################


# Install nginx. Maybe don't need curl and vim, though vim could potentially be handy.
# 'icu' packages are to support languages other than 'invariant'.
RUN apk update \
	&& apk add --no-cache nginx curl vim icu-data-full icu-libs

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

# EXPOSE 3500

COPY ./nginx.conf /usr/nginx.conf
COPY ./start.sh /usr/start.sh
RUN chmod +x /usr/start.sh

ARG IMAGE_VERSION
ENV IMAGE_VERSION $IMAGE_VERSION

ARG GIT_COMMIT
ENV GIT_COMMIT $GIT_COMMIT

# CMD ["node", "bundle.js"]
CMD ["/usr/start.sh"]
