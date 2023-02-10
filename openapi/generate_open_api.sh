rm -rf generated_ts_client

curl http://localhost:5127/swagger/v1/swagger.json> openapi_schema.json
openapi-generator generate -i openapi_schema.json -g typescript-axios -o generated_ts_client --global-property skipFormModel=false
#openapi-generator generate -i openapi_schema.json -g typescript-fetch -o generated_fetch_client

rsync -a generated_ts_client/api.ts ../frontend/src/client_api
rsync -a generated_ts_client/base.ts ../frontend/src/client_api
rsync -a generated_ts_client/common.ts ../frontend/src/client_api
rsync -a generated_ts_client/configuration.ts ../frontend/src/client_api
rsync -a generated_ts_client/index.ts ../frontend/src/client_api
