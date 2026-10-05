#!/usr/bin/env bash
# Adds a new EF Core migration for VoidwellDbContext.
# Usage: scripts/init-migrate.sh [migration-name]
# Without a name, the migration is named voidwelldbcontext.<timestamp>.
set -euo pipefail

cd "$(dirname "${BASH_SOURCE[0]}")/../src/Voidwell.Platform.Data"

export ASPNETCORE_ENVIRONMENT=Development

migration="${1:-voidwelldbcontext.$(date +%Y_%m_%d_%H_%M_%S)}"

dotnet ef migrations add "$migration" -v \
    -c Voidwell.Platform.Data.VoidwellDbContext \
    -o ./Migrations
