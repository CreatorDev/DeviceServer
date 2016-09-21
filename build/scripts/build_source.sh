#!/bin/bash
dotnet restore && dotnet build --configuration=Release /app/src/*
