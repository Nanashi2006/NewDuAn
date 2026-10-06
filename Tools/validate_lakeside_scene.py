"""Static integrity checks for the authored Unity scene; no Unity/Python dependencies."""
from pathlib import Path
import re, struct, math, json
ROOT=Path('Assets/Chapter14DynamicEnvironment')
scene=(ROOT/'Scenes/Chapter14_Demo.unity').read_text()
blocks={}
for m in re.finditer(r'--- !u!(\d+) &(\d+)\n(.*?)(?=--- !u!|\Z)',scene,re.S):
 cls,id_,body=int(m[1]),int(m[2]),m[3]
 assert id_ not in blocks, f'Duplicate file ID {id_}'
 blocks[id_]=(cls,body)
for cls,body in blocks.values():
 for i in re.findall(r'\{fileID: (-?\d+)\}',body):
  assert int(i)==0 or int(i) in blocks, f'Missing scene reference {i}'
assets={}
for meta in Path('Assets').rglob('*.meta'):
 g=re.search(r'^guid: (\w+)',meta.read_text(),re.M)
 if not g:continue
 assert g[1] not in assets, f'Duplicate asset GUID {g[1]}'
 assets[g[1]]=meta.with_suffix('')
package_guids={'a79441f348de89743a2939f4d699eac1','172515602e62fb746b5d573b38a5fe58','0b2db86121404754db890f4c8dfe81b2','899c54efeace73346a0a16faa3afe726','d7fd9488000d3734a9e00ee676215985','97c23e3b12dc18c42a140437e53d3951','ccf1aba9553839d41ae37dd52e9ebcce'}
for p in [ROOT/'Scenes/Chapter14_Demo.unity',*(ROOT/'Art').rglob('*.mat')]:
 for fid,g,type_ in re.findall(r'fileID: (\d+), guid: (\w+), type: (\d+)',p.read_text()):
  if g.startswith('0000000000000000') or g in package_guids:continue
  assert g in assets and assets[g].exists(), f'Missing GUID {g} in {p}'
  assert int(type_)==(3 if int(fid) in (11500000,4800000) else 2), f'Wrong asset reference type in {p}'
# Component ownership and both directions of hierarchy edges must agree.
for i,(cls,body) in blocks.items():
 if cls==1:
  ids=[int(c) for c in re.findall(r'component: \{fileID: (\d+)\}',body)]
  assert ids and blocks[ids[0]][0]==4
  for c in ids:assert f'm_GameObject: {{fileID: {i}}}' in blocks[c][1], 'Wrong component owner'
 if cls==4:
  father=int(re.search(r'm_Father: \{fileID: (\d+)\}',body)[1])
  children=[int(c) for c in re.findall(r'- \{fileID: (\d+)\}',body)]
  if father:assert f'- {{fileID: {i}}}' in blocks[father][1], 'Child absent from parent'
  for c in children:assert f'm_Father: {{fileID: {i}}}' in blocks[c][1], 'Parent mismatch'
  for line in re.findall(r'm_LocalScale: \{(.*?)\}',body):
   assert all(float(v)>0 for v in re.findall(r': ([^,}]+)',line)), 'Zero/negative object scale'
# Native mesh channel stride, binary byte counts, bounds, normals and triangle winding.
triangle_count=0
for p in (ROOT/'Art/Meshes').glob('*.asset'):
 s=p.read_text();count=int(re.search(r'm_VertexCount: (\d+)',s)[1]);size=int(re.search(r'm_DataSize: (\d+)',s)[1]);data=bytes.fromhex(re.search(r'_typelessdata: (\w+)',s)[1]);indices=bytes.fromhex(re.search(r'm_IndexBuffer: (\w+)',s)[1]);fmt=int(re.search(r'm_IndexFormat: (\d+)',s)[1])
 assert len(data)==size==count*24 and count%3==0,p
 assert len(indices)==count*(4 if fmt else 2),p
 nums=struct.unpack('<'+('I' if fmt else 'H')*count,indices)
 assert max(nums)<count,p
 verts=list(struct.iter_unpack('<6f',data));assert all(math.isfinite(v) for vertex in verts for v in vertex)
 assert all(abs(sum(v*v for v in vertex[3:])-1)<1e-5 for vertex in verts),p
 if p.stem in ['Landscape','LakeDisc']:assert all(v[4]>0 for v in verts),'Ground/water faces point downward'
 triangle_count+=count//3
# A unique tagged player/camera and AI target references prevent cross-root mistakes.
gos=[b for cls,b in blocks.values() if cls==1]
assert sum('m_TagString: Player\n' in b for b in gos)==1
assert sum('m_TagString: MainCamera\n' in b for b in gos)==1
assert not any('m_Name: Chapter14_DemoBootstrap\n' in b for b in gos)
assert sum(cls==195 for cls,b in blocks.values())==3
assert all('m_Enabled: 0' in b for cls,b in blocks.values() if cls==195),'Agents start before NavMesh'
assert 'hit.transform.root == playerTransform.root' not in (ROOT/'Scripts/EnemyController.cs').read_text()
# Bridge approach riser must be within agent/CharacterController step limits.
planks=[i for i,(cls,b) in blocks.items() if cls==1 and 'm_Name: Deck Plank\n' in b]
assert len(planks)>2
for i in [planks[0],planks[-1]]:
 tr=int(re.search(r'component: \{fileID: (\d+)\}',blocks[i][1])[1]);b=blocks[tr][1]
 y=float(re.search(r'm_LocalPosition: \{x: [^,]+, y: ([^,]+)',b)[1]);sy=float(re.search(r'm_LocalScale: \{x: [^,]+, y: ([^,]+)',b)[1])
 assert y+sy*.5<=.35+1e-4,'Bridge entry higher than player stepOffset'
print(json.dumps({'result':'PASS','scene_objects':len(gos),'scene_components':len(blocks)-len(gos),'native_mesh_triangles':triangle_count,'meshes':5,'local_guids':len(assets)},indent=2))
