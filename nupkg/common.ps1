# Paths
$packFolder = (Get-Item -Path "./" -Verbose).FullName
$rootFolder = Join-Path $packFolder "../"

# List of solutions
$solutions = (
    ""
)

# List of projects
$projects = (

    # src
    "src/AlemdarLabs.ColorPalette",
    "src/AlemdarLabs.ColorPalette.Shared"
)