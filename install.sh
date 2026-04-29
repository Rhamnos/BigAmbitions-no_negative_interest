#!/usr/bin/env bash
set -e

GAME_NAME="Big Ambitions"
PLUGIN_NAME="No Negative Interest"

echo "=== $PLUGIN_NAME installer ==="
echo ""

# Common Steam library locations on Linux
SEARCH_PATHS=(
    "$HOME/.steam/debian-installation/steamapps/common/$GAME_NAME"
    "$HOME/.steam/steam/steamapps/common/$GAME_NAME"
    "$HOME/.local/share/Steam/steamapps/common/$GAME_NAME"
    "$HOME/.steam/steamapps/common/$GAME_NAME"
)

GAME_DIR=""
for p in "${SEARCH_PATHS[@]}"; do
    if [ -f "$p/Big Ambitions.exe" ]; then
        GAME_DIR="$p"
        break
    fi
done

if [ -z "$GAME_DIR" ]; then
    echo "Could not auto-detect Big Ambitions install folder."
    echo "Please enter the full path to your Big Ambitions folder"
    echo "(the one containing 'Big Ambitions.exe'):"
    read -r GAME_DIR
    if [ ! -f "$GAME_DIR/Big Ambitions.exe" ]; then
        echo "ERROR: Big Ambitions.exe not found at '$GAME_DIR'. Aborting."
        exit 1
    fi
fi

echo "Found game at: $GAME_DIR"
echo "Installing $PLUGIN_NAME..."

SCRIPT_DIR="$(cd "$(dirname "$0")" && pwd)"
cp -r "$SCRIPT_DIR/payload/." "$GAME_DIR/"

echo ""
echo "=== Done! ==="
echo ""
echo "One final step — add this to your Big Ambitions Steam launch options:"
echo ""
echo "    WINEDLLOVERRIDES=\"winhttp=n,b\" %command%"
echo ""
echo "(Steam → Big Ambitions → Properties → General → Launch Options)"
echo ""
echo "Then launch the game. Check BepInEx/LogOutput.log in the game folder"
echo "to confirm the mod loaded."
