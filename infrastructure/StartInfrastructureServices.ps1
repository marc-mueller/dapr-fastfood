# docker create network fastfoodnetwork
# docker run -d -p 5672:5672 -p 15672:15672 --name ff-rabbitmq rabbitmq:3-management-alpine #--network fastfoodnetwork
# docker run -d -p 9411:9411 --name ff-zipkin openzipkin/zipkin #--network fastfoodnetwork
# docker run -d -p 6379:6379 --name ff-redis redis:latest --network fastfoodnetwork

docker compose -f ../docker-compose.yml up -d rabbitmq placement redis zipkin dapr-dashboard