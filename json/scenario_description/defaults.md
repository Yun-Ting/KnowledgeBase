# Default options

The default behaviour for migration should be as follows:

## Defaults to be used during migration

- Code before migration:

      this.logger.LogInformation($"Retrieved null application user");

- Code after migration:

      this.logger.LogInformation("Retrieved null application user");
