"""Checks scene references, the preserved village, real model bones/materials and gameplay controllers."""
from pathlib import Path
import re, json
ROOT=Path('Assets/Chapter14DynamicEnvironment')
OUT=Path('Assets/Scenes/LakesideVillage_Rebuilt.unity')

def parse(path):
    blocks={}
    for m in re.finditer(r'--- !u!(\d+) &(\d+)(?: stripped)?\n(.*?)(?=--- !u!|\Z)',path.read_text(),re.S):
        i=int(m[2]);assert i not in blocks, f'Duplicate ID {i}'
        blocks[i]=(int(m[1]),m[3])
    return blocks
blocks=parse(OUT);source=parse(ROOT/'Scenes/Chapter14_Demo.unity')
assets={}
for p in Path('Assets').rglob('*.meta'):
    m=re.search(r'^guid: (\w+)',p.read_text(),re.M)
    if m:
        assert m[1] not in assets, f'Duplicate GUID {m[1]}'
        assets[m[1]]=p.with_suffix('')
package_guids={'a79441f348de89743a2939f4d699eac1','172515602e62fb746b5d573b38a5fe58','0b2db86121404754db890f4c8dfe81b2','899c54efeace73346a0a16faa3afe726','d7fd9488000d3734a9e00ee676215985','97c23e3b12dc18c42a140437e53d3951','ccf1aba9553839d41ae37dd52e9ebcce'}
for cls,body in blocks.values():
    for value in re.findall(r'\{fileID: (-?\d+)\}',body):
        assert int(value)==0 or int(value) in blocks, f'Missing local reference {value}'
for p in [OUT,*(ROOT/'Generated').glob('*.mat'),*(ROOT/'Generated').glob('*.controller'),*(ROOT/'Generated').glob('*.prefab'),*Path('Assets/DogKnight/Material').glob('*.mat'),*Path('Assets/DacingEyebrows/deb_Goblin01/Materials').glob('*.mat')]:
    for fid,g,type_ in re.findall(r'fileID: (-?\d+), guid: (\w+), type: (\d+)',p.read_text()):
        if g.startswith('0000000000000000') or g in package_guids: continue
        assert g in assets and assets[g].exists(), f'Missing GUID {g} in {p}'
        if int(fid) in [11500000,4800000,2800000]: assert int(type_)==3, f'Incorrect importer type in {p}'
for i,(cls,b) in blocks.items():
    if cls==1:
        components=list(map(int,re.findall(r'component: \{fileID: (\d+)\}',b)))
        assert components and blocks[components[0]][0]==4
        for c in components: assert f'm_GameObject: {{fileID: {i}}}\n' in blocks[c][1], 'Wrong owner'
    if cls==4 and '  m_Father:' in b:
        father=int(re.search(r'm_Father: \{fileID: (\d+)\}',b)[1])
        children=list(map(int,re.findall(r'- \{fileID: (\d+)\}',b)))
        if father: assert f'- {{fileID: {i}}}\n' in blocks[father][1], f'Missing parent edge {i}'
        for c in children:
            cb=blocks[c][1]
            if '  m_Father:' in cb: assert f'm_Father: {{fileID: {i}}}\n' in cb, f'Missing child edge {c}'
            else:
                instance=int(re.search(r'm_PrefabInstance: \{fileID: (\d+)\}',cb)[1])
                assert f'm_TransformParent: {{fileID: {i}}}\n' in blocks[instance][1], 'Nested weapon parent mismatch'
# Every original scenery transform survives verbatim. The actor visuals are the only replaced geometry.
environment=next(i for i,(c,b) in source.items() if c==1 and 'm_Name: 01 Environment\n' in b)
envtr=int(re.search(r'component: \{fileID: (\d+)\}',source[environment][1])[1])
count=0

def preserve(i):
    global count
    assert blocks[i]==source[i], f'Scenery transform moved: {i}'
    count+=1
    for c in re.findall(r'- \{fileID: (\d+)\}',source[i][1]):
        # A new vegetation group is added under the existing forest, so compare its original transform fields separately.
        child=int(c)
        if 'm_Name: 06 Trees and Undergrowth\n' in source[int(re.search(r'm_GameObject: \{fileID: (\d+)\}',source[child][1])[1])][1]:
            old=source[child][1];new=blocks[child][1]
            for key in ['m_LocalPosition','m_LocalRotation','m_LocalScale','m_Father']:
                assert re.search(r'  '+key+r': .*',old)[0]==re.search(r'  '+key+r': .*',new)[0]
            for sub in re.findall(r'- \{fileID: (\d+)\}',old): preserve(int(sub))
        else: preserve(child)
preserve(envtr)
assert sum(c==95 for c,b in blocks.values())==4
assert sum(c==195 for c,b in blocks.values())==3
assert sum(c==1 and 'm_TagString: Player\n' in b for c,b in blocks.values())==1
assert sum(c==1 and 'm_TagString: MainCamera\n' in b for c,b in blocks.values())==1
assert all('m_Enabled: 0' in b for c,b in blocks.values() if c==195)
script_meta=(ROOT/'Scripts/LakesideAnimationDriver.cs.meta').read_text();g=re.search(r'guid: (\w+)',script_meta)[1]
assert sum(c==114 and g in b for c,b in blocks.values())==4
for name in ['DogKnight_Gameplay','Goblin_Gameplay']:
    s=(ROOT/'Generated'/(name+'.controller')).read_text()
    assert not re.search(r'  m_Transitions:\n  -',s), 'Automatic showcase transitions remain'
    assert 'm_AnimatorParameters: []' in s
    assert 'm_DefaultWeight: 1' in s
for p in [Path('Assets/DogKnight/Animations/Attack01.anim'),Path('Assets/DogKnight/Animations/Die.anim'),Path('Assets/DacingEyebrows/deb_Goblin01/Animation/Die01.anim')]:
    assert 'm_LoopTime: 0' in p.read_text(),f'Action still loops: {p}'
assert 'GraphicsSettings.currentRenderPipeline != null) return;' in Path('Assets/AQUAS-Lite/Scripts/AQUAS_Lite_Reflection.cs').read_text()
print(json.dumps({'result':'PASS','preserved_scenery_transforms':count,'animated_uploaded_characters':4,'scene_blocks':len(blocks),'vegetation_cards':sum(c==1 and ('m_Name: ALP Grass\n' in b or 'm_Name: ALP Flower\n' in b) for c,b in blocks.values())},indent=2))
