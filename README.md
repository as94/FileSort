# External File Sorter

A C# project for generating and externally sorting large text files containing numbers and strings.

---

## Features

- Generate large files of arbitrary size (B, KB, MB, GB)
- External sorting using chunked sorting + k-way merge
- Parallel chunk sorting for performance
- Modular core logic for easy testing

## Console Applications

- **Generator.Console** — generate files
- **Sorter.Console** — sort files

## Usage

### Generate Files

Generator.Console `<output-file>` `<size-in-B/KB/MB/GB>`
- `<output-file>` — output file path
- `<size-in-B/KB/MB/GB>` — file size as a number with unit (B, KB, MB, GB)

**Example:**

```bash
# Generate 100MB file
dotnet run --project Generator.Console -- "100MB.txt" 100MB
```

### Sort Files

Sorter.Console `<inputFile>` `<outputFile>`
- `<inputFile>` — path to the existing file
- `<outputFile>` — path for the sorted file

**Example:**
```bash
dotnet run --project Sorter.Console -- 100MB.txt 100MB_sorted.txt
```

## Architecture

- **Core**: contains file generation, chunk sorting, k-way merge
- **ChunkSorter**: sorts chunks of the file in parallel
- **ExternalMerger**: merges sorted chunks into the final file
- **Interfaces**: ILineReader/ILineWriter, IChunkSortAlgorithm, IMergeAlgorithm
- **Console apps**: wrap the Core for command-line use

## Tests

- Unit and integration tests exist for Core functionality
- Performance tests (large files) must be run manually

## GitHub Actions (CI)
- Workflow included for automatic build and test on push or pull request
- Tests run on ubuntu-latest using .NET 9 SDK
- Performance tests are skipped in CI to avoid long runs

## Performance (M2 Air, 8 cores)

These are approximate timings observed on an Apple M2 Air with 8 CPU cores:

| File Size | Time |
|-----------|------|
| 1 GB      | 19 seconds |
| 10 GB     | ~5.5 minutes |
| 100 GB    | ~1 hour 10 minutes |