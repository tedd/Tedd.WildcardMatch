"""Lossless storage of raw launch JSON; preserve every sample and original byte hash."""
from pathlib import Path
import gzip,hashlib,json
here=Path(__file__).resolve().parent
manifest=[]
for source in sorted(here.glob('*/launch-*.json')):
    assert here.resolve() in source.resolve().parents
    original=source.read_bytes()
    compressed=gzip.compress(original,mtime=0)
    target=source.with_suffix('.json.gz')
    assert not target.exists()
    target.write_bytes(compressed)
    assert gzip.decompress(target.read_bytes())==original
    manifest.append({'path':str(target.relative_to(here)).replace('\\','/'),'originalSha256':hashlib.sha256(original).hexdigest(),'originalBytes':len(original),'compressedBytes':len(compressed)})
    source.unlink()
(here/'raw-archive.json').write_text(json.dumps(manifest,indent=2)+'\n',encoding='utf-8')
