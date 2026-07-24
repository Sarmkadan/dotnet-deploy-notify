#!/usr/bin/env python3
"""
Utility script to build and run tests for the dotnet-deploy-notify project.
"""

import subprocess
import sys
from pathlib import Path

def run_dotnet_test():
    # Locate the solution file (.sln) in the current directory or its parent.
    cwd = Path(__file__).parent
    sln_files = list(cwd.glob("*.sln"))
    if not sln_files:
        sln_files = list((cwd.parent).glob("*.sln"))
    if not sln_files:
        print("Error: No solution (.sln) file found.", file=sys.stderr)
        sys.exit(1)

    sln_path = sln_files[0]
    cmd = ["dotnet", "test", str(sln_path), "--no-build", "--verbosity", "minimal"]
    try:
        subprocess.check_call(cmd)
    except subprocess.CalledProcessError as e:
        sys.exit(e.returncode)

if __name__ == "__main__":
    run_dotnet_test()
