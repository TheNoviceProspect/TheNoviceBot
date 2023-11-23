#!/usr/bin/env bash
echo "This script nukes all build outputs, make sure you want this!!!"
###
# -n defines the required character count to stop reading
# -s hides the user's input
# -r causes the string to be interpreted "raw" (without considering backslash escapes)
# -p The prompt to display before expecting input.
###
read -n 1 -s -r -p "Press any key to continue or CTRL+C to stop this script"
rm -vrf ./app/src/bin/ ./app/src/obj/
