"""Create an asset-based copy of the authored village without changing any scenery transforms.
Run from repository root. Uses only the standard library; does not require Unity to author YAML.
"""
from pathlib import Path
import re, hashlib, struct, math, random, json
ROOT = Path('Assets/Chapter14DynamicEnvironment')
GEN = ROOT/'Generated'
GEN.mkdir(exist_ok=True)

def meta(p, importer='DefaultImporter', fileid=None):
    p=Path(p); m=Path(str(p)+'.meta')
    if not m.exists():
        g=hashlib.md5(('NewDuAn/asset-repair/'+str(p)).encode()).hexdigest()
        m.write_text('fileFormatVersion: 2\nguid: '+g+'\n'+('folderAsset: yes\n' if p.is_dir() else '')+importer+':\n  externalObjects: {}\n'+(f'  mainObjectFileID: {fileid}\n' if fileid else '')+'  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
    return re.search(r'^guid: (\w+)',m.read_text(),re.M)[1]

def asset(p, fid=2100000): return f'{{fileID: {fid}, guid: {meta(p)}, type: {3 if fid in (11500000,4800000,2800000) else 2}}}'
meta(GEN)
for p in list((ROOT/'Scripts').glob('*.cs'))+list((ROOT/'Editor').glob('*.cs')): meta(p,'MonoImporter')
for p in (ROOT/'Art/Shaders').glob('*.shader'): meta(p,'ShaderImporter')

# Preserve source materials' texture links; replace only the incompatible shading implementation.
def convert_material(p):
    s=p.read_text()
    available={re.search(r'^guid: (\w+)',m.read_text(),re.M)[1] for m in Path('Assets').rglob('*.meta') if re.search(r'^guid: (\w+)',m.read_text(),re.M)}
    s=re.sub(r'm_Texture: \{fileID: (\d+), guid: (\w+), type: (\d+)\}', lambda m:m[0] if m[2] in available else 'm_Texture: {fileID: 0}',s)
    s=re.sub(r'  m_Shader: .*', '  m_Shader: '+asset(ROOT/'Art/Shaders/AssetLit.shader',4800000),s)
    main=re.search(r'    - _MainTex:\n(.*?)(?=    - |    m_Ints:)',s,re.S)
    if main and '    - _BaseMap:' not in s:
        s=s.replace('    m_TexEnvs:\n','    m_TexEnvs:\n    - _BaseMap:\n'+main[1],1)
    color=re.search(r'    - _Color: (.*)',s)
    if color and '    - _BaseColor:' not in s: s=s.replace('    m_Colors:\n','    m_Colors:\n    - _BaseColor: '+color[1]+'\n')
    gloss=re.search(r'    - _Glossiness: (.*)',s)
    if '    - _Smoothness:' not in s: s=s.replace('    m_Floats:\n','    m_Floats:\n    - _Smoothness: '+(gloss[1] if gloss else '0.3')+'\n')
    s=re.sub(r'  m_ValidKeywords:\n.*?(?=  m_InvalidKeywords:)', '  m_ValidKeywords: []\n',s,flags=re.S)
    p.write_text(s)
for folder in ['Assets/DogKnight/Material','Assets/DacingEyebrows/deb_Goblin01/Materials']:
    for p in Path(folder).glob('*.mat'):
        if 'Skybox' not in p.name: convert_material(p)

# Copy demo controllers, dropping automatic showcase transitions. Gameplay drives states explicitly.
def controller_copy(source, name):
    s=Path(source).read_text()
    s=re.sub(r'  m_Transitions:\n(?:  - .*\n)+','  m_Transitions: []\n',s)
    s=re.sub(r'  m_AnyStateTransitions:\n(?:  - .*\n)+','  m_AnyStateTransitions: []\n',s)
    s=re.sub(r'  m_EntryTransitions:\n(?:  - .*\n)+','  m_EntryTransitions: []\n',s)
    s=s.replace('    m_DefaultWeight: 0','    m_DefaultWeight: 1')
    p=GEN/(name+'.controller'); p.write_text(s); meta(p,'NativeFormatImporter',9100000); return p
DOG_CONTROLLER=controller_copy('Assets/DogKnight/Animator/DogControl.controller','DogKnight_Gameplay')
GOB_CONTROLLER=controller_copy('Assets/DacingEyebrows/deb_Goblin01/Animator/deb_Goblin01.controller','Goblin_Gameplay')
for p in [Path('Assets/DogKnight/Animations/Attack01.anim'),Path('Assets/DogKnight/Animations/Attack02.anim'),Path('Assets/DogKnight/Animations/Die.anim'),Path('Assets/DacingEyebrows/deb_Goblin01/Animation/Die01.anim'),Path('Assets/DacingEyebrows/deb_Goblin01/Animation/Attack01.anim')]:
    p.write_text(p.read_text().replace('m_LoopTime: 1','m_LoopTime: 0'))

# Lake material consumes AQUAS normals/foam without GrabPass or a nested Camera.Render.
def texture_block(prop, path, tiling=(1,1)):
    return f'    - {prop}:\n        m_Texture: '+asset(Path(path),2800000)+f'\n        m_Scale: {{x: {tiling[0]}, y: {tiling[1]}}}\n        m_Offset: {{x: 0, y: 0}}\n'
def material(p, shader, textures, floats='', colors='    - _BaseColor: {r: 1, g: 1, b: 1, a: 1}\n'):
    p.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!21 &2100000\nMaterial:\n  serializedVersion: 8\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_Name: '+p.stem+'\n  m_Shader: '+asset(shader,4800000)+'\n  m_ValidKeywords: []\n  m_InvalidKeywords: []\n  m_LightmapFlags: 4\n  m_EnableInstancingVariants: 0\n  m_DoubleSidedGI: 0\n  m_CustomRenderQueue: -1\n  stringTagMap: {}\n  disabledShaderPasses: []\n  m_SavedProperties:\n    serializedVersion: 3\n    m_TexEnvs:\n'+textures+'    m_Ints: []\n    m_Floats:\n'+floats+'    m_Colors:\n'+colors+'  m_BuildTextureStacks: []\n')
    text=p.read_text().replace('    m_TexEnvs:\n    m_Ints:', '    m_TexEnvs: []\n    m_Ints:').replace('    m_Floats:\n    m_Colors:', '    m_Floats: []\n    m_Colors:')
    p.write_text(text)
    meta(p,'NativeFormatImporter',2100000)
WATER=GEN/'Water_AQUAS_URP.mat'
material(WATER,ROOT/'Art/Shaders/LakeWater.shader',texture_block('_BumpMap','Assets/AQUAS-Lite/Textures/Waves_LoRes.png')+texture_block('_FoamMap','Assets/AQUAS-Lite/Textures/AQUAS_Foam.jpg')+'    - _BaseMap:\n        m_Texture: {fileID: 0}\n        m_Scale: {x: 1, y: 1}\n        m_Offset: {x: 0, y: 0}\n',colors='    - _BaseColor: {r: 0.055, g: 0.32, b: 0.4, a: 0.88}\n')
for p in [Path('Assets/AQUAS-Lite/Materials/AQUAS_Lite_Water.mat'),Path('Assets/AQUAS-Lite/Materials/AQUAS_Lite_Water_Backface.mat')]:
    # Source prefab and demo scenes also become safe under URP.
    p.write_text(WATER.read_text().replace('m_Name: Water_AQUAS_URP','m_Name: '+p.stem))
GROUND=GEN/'Ground_ALP_Handpainted.mat'
material(GROUND,ROOT/'Art/Shaders/LakesideLit.shader',texture_block('_GroundMap','Assets/ALP_Assets/GrassFlowersFREE/Textures/Ground/Grass01_BigUV.png')+texture_block('_BankMap','Assets/Handpainted_Grass_and_Ground_Textures/Textures/Dirt/dirt_clay/dirt_clay_up.png'),'    - _Ground: 1\n    - _TextureWeight: 0.85\n    - _Smoothness: 0.04\n    - _Cull: 2\n    - _Cutoff: 0.5\n')
FLOWER_MATS=[]
for name in ['grass01','grass02','grassFlower03','grassFlower01']:
    p=GEN/(name+'_URP.mat')
    material(p,ROOT/'Art/Shaders/AssetLit.shader',texture_block('_BaseMap',f'Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/{name}.tga'),'    - _AlphaClip: 1\n    - _Cutoff: 0.35\n    - _Cull: 0\n    - _Smoothness: 0\n')
    FLOWER_MATS.append(p)
# Keep the uploaded Rainy VFX particle asset, replacing its unsupported default material.
RAIN_MAT=GEN/'RainyVFX_URP.mat'
material(RAIN_MAT,ROOT/'Art/Shaders/RainParticles.shader','',colors='    - _BaseColor: {r: 0.7, g: 0.84, b: 1, a: 0.55}\n')
RAIN_PREFAB=GEN/'RainyVFX_URP.prefab'
rain=Path('Assets/Rainy VFX/Prefab/Particle System (1).prefab').read_text()
rain=re.sub(r'(  m_Materials:\n)(?:  - .*\n)+',r'\g<1>  - '+asset(RAIN_MAT)+'\n',rain)
RAIN_PREFAB.write_text(rain);meta(RAIN_PREFAB,'PrefabImporter')
RAIN_ROOT=int(re.search(r'--- !u!1 &(\d+)',rain)[1])
# UV mesh for grass cards, using the original landscape mesh serialization as a safe template.
QUAD=GEN/'VegetationCard.asset'
mesh=(ROOT/'Art/Meshes/LakeDisc.asset').read_text()
verts=[(-.5,0,0,0,0,1,0,0),(.5,0,0,0,0,1,1,0),(.5,1,0,0,0,1,1,1),(-.5,1,0,0,0,1,0,1)]
data=b''.join(struct.pack('<8f',*v) for v in verts)
indices=struct.pack('<6H',0,1,2,0,2,3)
mesh=mesh.replace('m_Name: LakeDisc','m_Name: VegetationCard')
mesh=re.sub(r'indexCount: \d+','indexCount: 6',mesh);mesh=re.sub(r'vertexCount: \d+','vertexCount: 4',mesh)
mesh=re.sub(r'm_VertexCount: \d+','m_VertexCount: 4',mesh);mesh=re.sub(r'm_DataSize: \d+','m_DataSize: 128',mesh)
mesh=re.sub(r'm_IndexBuffer: \w+','m_IndexBuffer: '+indices.hex(),mesh);mesh=re.sub(r'_typelessdata: \w+','_typelessdata: '+data.hex(),mesh)
mesh=re.sub(r'm_Center: \{[^}]*\}', 'm_Center: {x: 0, y: 0.5, z: 0}',mesh)
mesh=re.sub(r'm_Extent: \{[^}]*\}', 'm_Extent: {x: 0.5, y: 0.5, z: 0.01}',mesh)
channels=''.join(f'    - stream: 0\n      offset: {12 if i==1 else 24 if i==4 else 0}\n      format: 0\n      dimension: {3 if i<2 else 2 if i==4 else 0}\n' for i in range(14))
mesh=re.sub(r'    m_Channels:\n.*?(?=    m_DataSize:)', '    m_Channels:\n'+channels,mesh,flags=re.S)
QUAD.write_text(mesh);meta(QUAD,'NativeFormatImporter',4300000)

SOURCE=ROOT/'Scenes/Chapter14_Demo.unity'
raw=SOURCE.read_text()
header=raw.split('--- !u!1 &')[0]
blocks={int(m[2]):[int(m[1]),m[3]] for m in re.finditer(r'--- !u!(\d+) &(\d+)\n(.*?)(?=--- !u!|\Z)',raw[len(header):],re.S)}
nextid=30000
stripped_ids=set()

def alloc():
    global nextid
    while nextid in blocks: nextid+=1
    val=nextid;nextid+=1;return val

def get_name(body):
    m=re.search(r'^  m_Name: (.*)$',body,re.M);return m[1] if m else ''
def go_named(name): return next(i for i,(cls,b) in blocks.items() if cls==1 and get_name(b)==name)
def tr_for(go): return int(re.search(r'component: \{fileID: (\d+)\}',blocks[go][1])[1])
def component(go, cls): return next((i for i,(c,b) in blocks.items() if c==cls and f'm_GameObject: {{fileID: {go}}}\n' in b),None)
def edit(i,key,value): blocks[i][1]=re.sub(r'^  '+re.escape(key)+r': .*$', '  '+key+': '+value,blocks[i][1],flags=re.M)
def append_child(parent,child):
    b=blocks[parent][1]
    if '  m_Children: []' in b: b=b.replace('  m_Children: []','  m_Children:\n  - {fileID: '+str(child)+'}')
    else: b=b.replace('  m_Father:','  - {fileID: '+str(child)+'}\n  m_Father:',1)
    blocks[parent][1]=b
COMMON='  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n'

def new_go(name,parent,pos=(0,0,0),scale=(1,1,1),yaw=0,layer=0):
    go,tr=alloc(),alloc()
    blocks[go]=[1,'GameObject:\n'+COMMON+f'  serializedVersion: 6\n  m_Component:\n  - component: {{fileID: {tr}}}\n  m_Layer: {layer}\n  m_Name: {name}\n  m_TagString: Untagged\n  m_Icon: {{fileID: 0}}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: 0\n  m_IsActive: 1\n']
    v=lambda a: '{x: '+str(a[0])+', y: '+str(a[1])+', z: '+str(a[2])+'}'
    blocks[tr]=[4,'Transform:\n'+COMMON+f'  m_GameObject: {{fileID: {go}}}\n  serializedVersion: 2\n  m_LocalRotation: {{x: 0, y: {math.sin(math.radians(yaw)/2)}, z: 0, w: {math.cos(math.radians(yaw)/2)}}}\n  m_LocalPosition: '+v(pos)+'\n  m_LocalScale: '+v(scale)+f'\n  m_Children: []\n  m_Father: {{fileID: {parent}}}\n  m_LocalEulerAnglesHint: {{x: 0, y: {yaw}, z: 0}}\n']
    append_child(parent,tr);return go,tr

def add_component(go,cls,type_,fields):
    i=alloc();blocks[i]=[cls,type_+':\n'+COMMON+f'  m_GameObject: {{fileID: {go}}}\n'+fields]
    blocks[go][1]=blocks[go][1].replace('  m_Layer:',f'  - component: {{fileID: {i}}}\n  m_Layer:',1);return i

def script(go,name,fields=''):
    return add_component(go,114,'MonoBehaviour','  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: '+asset(ROOT/'Scripts'/(name+'.cs'),11500000)+'\n  m_Name:\n  m_EditorClassIdentifier:\n'+fields)

def drop_tree(tr):
    cls,body=blocks[tr]
    for child in list(map(int,re.findall(r'- \{fileID: (\d+)\}',body))): drop_tree(child)
    go=int(re.search(r'm_GameObject: \{fileID: (\d+)\}',body)[1]);comps=map(int,re.findall(r'component: \{fileID: (\d+)\}',blocks[go][1]))
    for c in list(comps): blocks.pop(c,None)
    blocks.pop(go,None)

# Material references and every environment transform come directly from the user's preferred scene.
ground_id=go_named('01 Landscape and Lake Basin')
renderer=component(ground_id,23)
old=asset(ROOT/'Art/Materials/M_Grass.mat')
blocks[renderer][1]=blocks[renderer][1].replace(old,asset(GROUND))
world=go_named('Lakeside Village');world_tr=tr_for(world)
edit(world,'m_Name','Lakeside Village - Asset Rebuild')
actors=[i for i,(cls,b) in blocks.items() if cls==1 and (get_name(b).startswith('Player -') or get_name(b).startswith('Enemy -'))]

# Flatten uploaded prefab hierarchy into the scene with unique local IDs, keeping meshes/bones/texture GUIDs.
def clone_prefab(path,parent,name,controller=None):
    txt=Path(path).read_text()
    parts=[(int(m[1]),int(m[2]),m[4],bool(m[3])) for m in re.finditer(r'--- !u!(\d+) &(\d+)( stripped)?\n(.*?)(?=--- !u!|\Z)',txt,re.S)]
    ids={old:alloc() for cls,old,body,strip in parts}
    root=next(old for cls,old,b,strip in parts if cls==4 and 'm_Father: {fileID: 0}' in b)
    root_go=int(re.search(r'm_GameObject: \{fileID: (\d+)\}',next(b for c,o,b,strip in parts if o==root))[1])
    animator=None
    for cls,old,b,strip in parts:
        b=re.sub(r'\{fileID: (-?\d+)\}',lambda m:'{fileID: '+str(ids.get(int(m[1]),int(m[1])))+'}',b)
        if old==root:
            b=re.sub(r'm_Father: \{fileID: 0\}',f'm_Father: {{fileID: {parent}}}',b)
            b=re.sub(r'  m_LocalPosition: .*','  m_LocalPosition: {x: 0, y: 0, z: 0}',b)
            b=re.sub(r'  m_LocalRotation: .*','  m_LocalRotation: {x: 0, y: 0, z: 0, w: 1}',b)
        if old==root_go: b=re.sub(r'  m_Name: .*','  m_Name: '+name,b)
        if cls==95:
            animator=ids[old]
            if controller: b=re.sub(r'  m_Controller: .*','  m_Controller: '+asset(controller,9100000),b)
            b=re.sub(r'  m_ApplyRootMotion: .*','  m_ApplyRootMotion: 0',b)
        blocks[ids[old]]=[cls,b]
        if strip: stripped_ids.add(ids[old])
    append_child(parent,ids[root]);return ids[root_go],ids[root],animator

for actor in actors:
    tr=tr_for(actor);old_visual=next(t for t,(c,b) in blocks.items() if c==4 and f'm_Father: {{fileID: {tr}}}\n' in b)
    blocks[tr][1]=blocks[tr][1].replace(f'  - {{fileID: {old_visual}}}\n','')
    drop_tree(old_visual)
    pose_guid=meta(ROOT/'Scripts/CharacterPose.cs')
    for i,(c,b) in list(blocks.items()):
        if c==114 and f'm_GameObject: {{fileID: {actor}}}\n' in b and pose_guid in b:
            del blocks[i];blocks[actor][1]=blocks[actor][1].replace(f'  - component: {{fileID: {i}}}\n','')
    enemy=get_name(blocks[actor][1]).startswith('Enemy')
    prefab='Assets/DacingEyebrows/deb_Goblin01/Prefab/deb_Goblin01.prefab' if enemy else 'Assets/DogKnight/Prefab/DogPBR.prefab'
    go,vtr,anim=clone_prefab(prefab,tr,'Visual',GOB_CONTROLLER if enemy else DOG_CONTROLLER)
    script(actor,'LakesideAnimationDriver',f'  animator: {{fileID: {anim}}}\n  idleState: '+('Idle01' if enemy else 'Idle_Battle')+'\n  walkState: '+('Move01' if enemy else 'WalkForwardBattle')+'\n  runState: '+('Move01' if enemy else 'RunForwardBattle')+'\n  attackState: Attack01\n  dieState: '+('Die01' if enemy else 'Die')+'\n')
    edit(actor,'m_Name',get_name(blocks[actor][1]).replace('Village Knight','DogKnight').replace('Enemy -','Goblin -'))
    if enemy:
        ec_guid=meta(ROOT/'Scripts/EnemyController.cs')
        for i,(c,b) in blocks.items():
            if c==114 and f'm_GameObject: {{fileID: {actor}}}\n' in b and ec_guid in b: edit(i,'chaseRange','5')
player=next(i for i in actors if 'm_TagString: Player' in blocks[i][1]);player_tr=tr_for(player)

# Keep the original shaped water mesh so the shoreline and bridge line up exactly; use real AQUAS textures.
water=go_named('Lake Surface');r=component(water,23)
blocks[r][1]=re.sub(r'(  m_Materials:\n  - ).*',r'\g<1>'+asset(WATER),blocks[r][1])
edit(water,'m_Name','Lake Surface - AQUAS Waves and Foam URP')

# HDR sky texture is already compatible; both runtime and clock reference it without changing the day cycle.
sky=Path('Assets/Free HDR Skyboxes Pack/Material/sky-2.mat')
for i,(c,b) in blocks.items():
    if c==114 and 'skyMaterial:' in b: edit(i,'skyMaterial',asset(sky))
    if c==114 and 'skyboxMaterial:' in b: edit(i,'skyboxMaterial',asset(sky))
header=re.sub(r'  m_SkyboxMaterial: .*','  m_SkyboxMaterial: '+asset(sky),header)

weather_guid=meta(ROOT/'Scripts/WeatherManager.cs')
for i,(c,b) in list(blocks.items()):
    if c==114 and weather_guid in b:
        reference=asset(RAIN_PREFAB,RAIN_ROOT)
        if '  rainParticlePrefab:' in b: edit(i,'rainParticlePrefab',reference)
        else: blocks[i][1]+='  rainParticlePrefab: '+reference+'\n'

# New vegetation stays off the existing stone paths, houses, lake, plaza, and guard camp.
forest=tr_for(go_named('06 Trees and Undergrowth'))
veg,vegtr=new_go('ALP Grass and Flowers - Imported Textures',forest)
def height(x,z):
    r=math.hypot((x-12)/17,(z-10)/13);edge=max(0,(max(abs(x),abs(z))-39)/21)
    return (-3.2*(1-r*r) if r<1 else 0)+edge*edge*(7+3*math.sin(x*.13)*math.cos(z*.1))
def mesh_renderer(go,path):
    template=blocks[r][1]
    template=re.sub(r'm_GameObject: \{fileID: \d+\}',f'm_GameObject: {{fileID: {go}}}',template)
    template=template.split('MeshRenderer:\n',1)[1]
    template=template[len(COMMON):].split('\n',1)[1] # drop common fields and the old owner, add_component inserts them.
    template=re.sub(r'(  m_Materials:\n  - ).*',r'\g<1>'+asset(path),template)
    template=template.replace('m_CastShadows: 0','m_CastShadows: 0')
    add_component(go,23,'MeshRenderer',template)
rand=random.Random(1406);patches=0
for _ in range(1000):
    x,z=rand.uniform(-42,42),rand.uniform(-35,39)
    if math.hypot((x-12)/17,(z-10)/13)<1.08 or abs(x+6)<4 or abs(z+7)<2.5 or (x<-10 and z>-13) or (x>26 and z<-2) or (abs(x)<5 and abs(z)<6): continue
    flower=rand.random()<0.18;idx=rand.choice([2,3] if flower else [0,1]);scale=rand.uniform(.42,.78)
    for angle in [rand.uniform(0,180),90]:
        go,tr=new_go('ALP Flower' if flower else 'ALP Grass',vegtr,(x,height(x,z)+.02,z),(scale,scale,scale),angle)
        add_component(go,33,'MeshFilter','  m_Mesh: '+asset(QUAD,4300000)+'\n');mesh_renderer(go,FLOWER_MATS[idx])
    patches+=1

# Physics trigger requires a Rigidbody on one participant; kinematic mud also works with NavMeshAgents.
characters=tr_for(go_named('02 Characters'))
mud,mudtr=new_go('Mud - Enemy Slowdown 1.5m per second',characters,(-1,0.03,24),(1,1,1))
add_component(mud,65,'BoxCollider','  m_Material: {fileID: 0}\n  m_IsTrigger: 1\n  m_Enabled: 1\n  serializedVersion: 3\n  m_Size: {x: 7, y: 3, z: 7}\n  m_Center: {x: 0, y: 1, z: 0}\n')
add_component(mud,54,'Rigidbody','  serializedVersion: 4\n  m_Mass: 1\n  m_UseGravity: 0\n  m_IsKinematic: 1\n  m_Interpolate: 0\n  m_Constraints: 0\n  m_CollisionDetection: 0\n')
script(mud,'LakesideMudZone')
# Surface uses a thin authored box renderer and texture, without a solid collider.
surface,st=new_go('Mud Surface',mudtr,(0,0,0),(7,.05,7))
add_component(surface,33,'MeshFilter','  m_Mesh: {fileID: 10202, guid: 0000000000000000e000000000000000, type: 0}\n')
MUD=GEN/'Mud_Handpainted.mat';material(MUD,ROOT/'Art/Shaders/LakesideLit.shader',texture_block('_BankMap','Assets/Handpainted_Grass_and_Ground_Textures/Textures/Dirt/dirt_clay/dirt_clay_up.png'),'    - _Ground: 0\n    - _Smoothness: 0.05\n',colors='    - _BaseColor: {r: 0.31, g: 0.2, b: 0.12, a: 1}\n')
mesh_renderer(surface,MUD)

# Saved camera plus a runtime-owned render target (no additional assets or camera stacks).
cam=go_named('Main Camera');camera_body=blocks[component(cam,20)][1]
mapgo,maptr=new_go('Minimap Camera',tr_for(go_named('04 Camera and Interface')),(-6,32,-24))
edit(maptr,'m_LocalRotation','{x: 0.7071068, y: 0, z: 0, w: 0.7071068}')
fields=camera_body.split('Camera:\n',1)[1];fields=fields[len(COMMON):].split('\n',1)[1]
fields=re.sub(r'^  orthographic: .*','  orthographic: 1',fields,flags=re.M)
fields=re.sub(r'^  orthographic size: .*','  orthographic size: 22',fields,flags=re.M)
fields=re.sub(r'^  m_Depth: .*','  m_Depth: -2',fields,flags=re.M)
fields=fields.replace('m_TargetTexture: {fileID: 0}','m_TargetTexture: {fileID: 0}')
add_component(mapgo,20,'Camera',fields);script(mapgo,'LakesideMinimapFollow',f'  target: {{fileID: {player_tr}}}\n  height: 32\n');script(mapgo,'LakesideMinimapView')
# Audit references in Inspector and Hierarchy without technical banners in the game's HUD.
audit,atr=new_go('00 Assets - DogKnight Goblin AQUAS ALP HDR',world_tr)
for label,path in [('DogKnight',Path('Assets/DogKnight/Prefab/DogPBR.prefab')),('Goblin',Path('Assets/DacingEyebrows/deb_Goblin01/Prefab/deb_Goblin01.prefab')),('AQUAS Waves',Path('Assets/AQUAS-Lite/Textures/Waves_LoRes.png')),('ALP Grass Flowers',Path('Assets/ALP_Assets/GrassFlowersFREE/Textures/GrassFlowers/grass01.tga')),('HDR Sky',sky),('Rainy VFX URP',RAIN_PREFAB)]:
    assert path.exists(),path
    new_go(label+' - '+path.name,atr)

OUT=Path('Assets/Scenes/LakesideVillage_Rebuilt.unity')
OUT.write_text(header+''.join(f'--- !u!{cls} &{i}'+(' stripped' if i in stripped_ids else '')+f'\n{b}' for i,(cls,b) in blocks.items()))
meta(OUT,'DefaultImporter')
settings=Path('ProjectSettings/EditorBuildSettings.asset');s=settings.read_text()
s=re.sub(r'  - enabled: [01]\n    path: Assets/Scenes/LakesideVillage_Rebuilt.unity\n    guid: .*\n','',s)
s=re.sub(r'  - enabled: 1\n    path: .*?\n    guid: .*?\n(?=  - |  m_configObjects:)',lambda m:m[0].replace('enabled: 1','enabled: 0'),s)
entry='  - enabled: 1\n    path: '+str(OUT)+'\n    guid: '+meta(OUT)+'\n'
s=s.replace('  m_Scenes:\n','  m_Scenes:\n'+entry)
settings.write_text(s)
for p in [OUT,*GEN.iterdir()]:
    if p.is_file(): p.write_text('\n'.join(line.rstrip() for line in p.read_text().splitlines())+'\n')
print(json.dumps({'scene':str(OUT),'actors':len(actors),'vegetation_patches':patches,'blocks':len(blocks)}))
