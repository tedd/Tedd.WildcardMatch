"""Hold one outer performance-lock acquisition across final dependent validation."""
from pathlib import Path
import subprocess
import sys
HERE = Path(__file__).resolve().parent
for command in ([sys.executable, "-B", str(HERE / "run.py"), "final"],
                [sys.executable, "-B", str(HERE / "run.py"), "reprofile"],
                [sys.executable, "-B", str(HERE / "release_check.py")]):
    subprocess.run(command, check=True)
