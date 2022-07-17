FROM mcr.microsoft.com/dotnet/sdk:6.0 as build-env

WORKDIR /app
COPY . ./
RUN dotnet publish ./DocFxToMarkdown/DocFxToMarkdown.csproj -c Release -o out --no-self-contained

FROM mcr.microsoft.com/dotnet/runtime:6.0
COPY --from=build-env /app/out .
ENTRYPOINT [ "dotnet", "/DocFxToMarkdown.dll" ]