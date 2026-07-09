FROM node:20-alpine AS build
WORKDIR /app

COPY Beneficios.Front/package.json Beneficios.Front/package-lock.json ./
RUN npm ci

COPY Beneficios.Front/ ./
RUN npm run build -- --configuration=docker

FROM nginx:1.27-alpine AS final

COPY Beneficios.Front/docker/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist/beneficios.front/browser /usr/share/nginx/html

EXPOSE 80

HEALTHCHECK --interval=30s --timeout=5s --start-period=10s --retries=3 \
  CMD test -f /usr/share/nginx/html/index.html || exit 1
