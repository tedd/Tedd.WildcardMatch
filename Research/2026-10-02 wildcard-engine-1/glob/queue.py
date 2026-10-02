"""Serial, predeclared local experiment queue. Run under the host lock."""
from pathlib import Path
import subprocess, sys
here=Path(__file__).resolve().parent
queues={
    'core': [
        ['H-039-v2','--variant','H-039','--profile'],
        ['H-037-v2','--variant','H-037','--control-variant','H-032'],
        ['H-036-v2','--variant','H-036','--control-variant','H-035'],
        ['H-038-v2','--variant','H-038','--control-variant','H-032'],
    ],
    'layout': [
        ['H-051-v2','--variant','H-051','--control-variant','H-035'],
        ['H-054-v2','--variant','H-054','--control-variant','H-036'],
        ['H-056-v2','--variant','H-056','--control-variant','H-048'],
        ['H-055-v2','--variant','H-055','--control-variant','H-041'],
        ['H-057-v2','--variant','H-057','--control-variant','H-055'],
    ],
    'contrasts': [
        ['H-046-v2','--variant','H-046','--control-variant','H-035'],
        ['H-045-v2','--variant','H-045','--control-variant','H-039'],
        ['H-048-v2','--variant','H-048','--control-variant','H-045'],
        ['H-040-v2','--variant','H-040'],
        ['H-041-v2','--variant','H-041','--control-variant','H-040'],
        ['H-042-v2','--variant','H-042'],
        ['H-044-v2','--variant','H-044','--control-variant','H-035','--cache-contrasts'],
        ['H-052-v2','--variant','H-052','--control-variant','H-037'],
        ['H-053-v2','--variant','H-053','--control-variant','H-037'],
    ]
}
for task in queues[sys.argv[1]]:
    if (here/task[0]/'identity.json').exists():
        print('ALREADY COMPLETE',task[0],flush=True)
        continue
    print('QUEUE',task,flush=True)
    subprocess.run([sys.executable,'-B',str(here/'run.py'),*task],check=True)
    subprocess.run([sys.executable,'-B',str(here/'summarize.py'),task[0]],check=True)
