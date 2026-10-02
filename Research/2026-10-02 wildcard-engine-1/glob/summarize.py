"""Summarize raw paired samples without mixing epochs or API lifetimes."""
import gzip, json, statistics, sys
from pathlib import Path
from collections import defaultdict
here=Path(__file__).resolve().parent
for epoch in sys.argv[1:]:
    groups=defaultdict(lambda:defaultdict(list))
    perlaunch=defaultdict(lambda:defaultdict(dict))
    for file in sorted((here/epoch).glob('launch-*.json*')):
        data=json.loads(gzip.decompress(file.read_bytes()) if file.suffix=='.gz' else file.read_bytes())
        local=defaultdict(lambda:defaultdict(list))
        for row in data['samples']:
            key=(row['fixture'],row['mode'])
            groups[key][row['engine']].append(row)
            local[key][row['engine']].append(row['ns'])
        for key,engines in local.items():
            for engine,values in engines.items(): perlaunch[key][engine][data['launch']]=statistics.median(values)
    summary=[]
    for (fixture,mode),engines in groups.items():
        med={engine:statistics.median(r['ns'] for r in rows) for engine,rows in engines.items()}
        row=dict(fixture=fixture,mode=mode,ns=med,launchMedians=perlaunch[(fixture,mode)],
            bytes={engine:statistics.median(r['bytes'] for r in rows) for engine,rows in engines.items()},
            samples={engine:len(rows) for engine,rows in engines.items()})
        row['controlOverCandidate']=med['control']/med['candidate']
        if 'glob' in med: row['globOverCandidate']=med['glob']/med['candidate']
        summary.append(row)
        if mode=='reused' or (fixture in ('Literal','Simple','MultiStar','LongText') and mode in ('static','construction')):
            print(f"{epoch:18} {fixture:16} {mode:12} "+' '.join(f'{e}={n:.2f}' for e,n in med.items())+f" speedup={row['controlOverCandidate']:.2f} glob={row.get('globOverCandidate',0):.2f}")
    (here/epoch/'summary.json').write_text(json.dumps(summary,indent=2))
