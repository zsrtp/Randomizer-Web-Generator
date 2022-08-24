#!/usr/bin/env sh

# This file must have LF line endings!!

nginx -c /usr/nginx.conf

node bundle.js
