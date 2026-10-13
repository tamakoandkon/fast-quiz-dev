# Build context: FastQuiz/
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080
RUN mkdir -p /app/data /app/logs && chown -R app:app /app/data /app/logs
USER $APP_UID

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY global.json Directory.Packages.props ./
COPY FastQuiz.API/FastQuiz.API.csproj FastQuiz.API/
COPY FastQuiz.Auth/FastQuiz.Auth.csproj FastQuiz.Auth/
COPY FastQuiz.Common/FastQuiz.Common.csproj FastQuiz.Common/
COPY FastQuiz.Question/FastQuiz.Question.csproj FastQuiz.Question/
COPY FastQuiz.Quiz/FastQuiz.Quiz.csproj FastQuiz.Quiz/
RUN dotnet restore FastQuiz.API/FastQuiz.API.csproj
COPY . .
RUN dotnet publish FastQuiz.API/FastQuiz.API.csproj -c $BUILD_CONFIGURATION \
    --no-restore -o /app/publish /p:UseAppHost=false

FROM base AS final
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "FastQuiz.API.dll"]
