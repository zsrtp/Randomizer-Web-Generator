#!/usr/bin/env sh

# This file must have LF line endings!! If you see a "no such file or directory"
# in Docker, it is probably because this file has CRLF line endings.

nginx -c /usr/nginx.conf

node bundle.js
