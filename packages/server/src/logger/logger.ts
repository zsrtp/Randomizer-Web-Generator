import winston from 'winston';
import { resolveOutputPath } from 'src/config';

import DailyRotateFile = require('winston-daily-rotate-file');

const { createLogger, format, transports } = winston;
const { json, combine, timestamp, errors } = format;

const outputDir = resolveOutputPath('logs');

let logger: winston.Logger;

const transport1 = new DailyRotateFile({
  filename: 'application-%DATE%.log',
  dirname: outputDir,
  datePattern: 'YYYY-MM-DD',
  zippedArchive: true,
  maxSize: '20m',
  maxFiles: '14d',
});

const transport2 = new DailyRotateFile({
  filename: 'errors-%DATE%.log',
  dirname: outputDir,
  datePattern: 'YYYY-MM-DD',
  zippedArchive: true,
  maxSize: '20m',
  maxFiles: '14d',
  level: 'error',
});

if (true || process.env.NODE_ENV === 'development') {
  // logger = createLogger({
  //   level: 'debug',
  //   // Order does matter for format combine
  //   format: combine(errors({ stack: true }), timestamp(), json()),
  //   transports: [
  //     new transports.File({
  //       dirname: outputDir,
  //       filename: 'error.log',
  //       level: 'error',
  //     }),
  //     new transports.File({
  //       dirname: outputDir,
  //       filename: 'combined.log',
  //     }),
  //   ],
  // });

  const consoleFormat = format.printf(({ level, message, timestamp }) => {
    return `${timestamp} ${level}: ${message}`;
  });

  logger = createLogger({
    level: 'debug',
    // Order does matter for format.combine
    format: combine(errors(), timestamp(), json()),
    transports: [
      transport1,
      transport2,
      new transports.Console({
        format: combine(
          format.colorize(),
          timestamp({ format: 'MM-DD HH:mm:ss.SSS' }),
          consoleFormat
        ),
      }),
    ],
  });
} else {
  // production
  logger = createLogger({
    level: 'debug',
    // Order does matter for format combine
    format: combine(errors({ stack: true }), timestamp(), json()),
    transports: [transport1, transport2],
  });
}

export default logger;
