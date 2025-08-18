# STAGE 1: Build Angular
FROM node:20 AS build
WORKDIR /app
COPY QuickChat.Client/package*.json ./
RUN npm ci
COPY QuickChat.Client/. .
RUN npm run build -- --output-path=dist --output-hashing=all

# STAGE 2: Serve with Nginx
FROM nginx:alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY QuickChat.Client/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]