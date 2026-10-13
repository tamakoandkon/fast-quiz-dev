import { fileURLToPath } from 'node:url';
import { generateApi } from 'swagger-typescript-api';

// Development defaults to local HTTP; never disable TLS certificate verification.
const url = process.env.FASTQUIZ_OPENAPI_URL || 'http://localhost:5026/swagger/v1/swagger.json';
await generateApi({
  url,
  output: fileURLToPath(new URL('../app/utils', import.meta.url)),
  fileName: 'backEnd.ts',
  httpClientType: 'axios',
  generateRouteTypes: true,
});
