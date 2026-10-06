"""Reproducible authoring utility; Unity loads the committed scene and assets directly.
Run from repository root: python Tools/build_lakeside_scene.py
No runtime map generation, third-party assets, or Python dependencies.
"""
from pathlib import Path
import math, random, hashlib, struct, re, json
ROOT = Path('Assets/Chapter14DynamicEnvironment')
random.seed(1406)

def guid(p): return hashlib.md5(('NewDuAn/lakeside/'+str(p)).encode()).hexdigest()
def meta(p, importer='DefaultImporter', fileid=None):
 p=Path(p); m=p.with_name(p.name+'.meta')
 if m.exists(): return re.search(r'guid: (\w+)',m.read_text())[1]
 folder=p.is_dir()
 m.write_text('fileFormatVersion: 2\nguid: '+guid(p)+'\n'+('folderAsset: yes\n' if folder else '')+importer+':\n  externalObjects: {}\n'+(f'  mainObjectFileID: {fileid}\n' if fileid else '')+'  userData:\n  assetBundleName:\n  assetBundleVariant:\n')
 return guid(p)
for p in [ROOT/'Art', ROOT/'Art/Meshes', ROOT/'Art/Materials', ROOT/'Art/Shaders', ROOT/'Editor']:
 p.mkdir(parents=True,exist_ok=True);meta(p)
for p in list((ROOT/'Scripts').glob('*.cs'))+list((ROOT/'Editor').glob('*.cs')): meta(p,'MonoImporter')
for p in (ROOT/'Art/Shaders').glob('*.shader'):meta(p,'ShaderImporter')

def vec(v, keys='xyz'): return '{'+', '.join(k+': '+f'{n:.7g}' for k,n in zip(keys,v))+'}'
def ref(i):return '{fileID: '+str(i)+'}'
def asset(g,i): return '{fileID: '+str(i)+', guid: '+g+', type: '+str(3 if i in (11500000,4800000) else 2)+'}'
materials={}
colors={'Grass':(.3,.44,.21), 'Plaster':(.83,.74,.55), 'PlasterCream':(.9,.84,.69), 'Wood':(.25,.13,.075), 'Planks':(.48,.29,.14), 'WoodLight':(.62,.43,.23), 'Roof':(.47,.19,.12), 'RoofLight':(.64,.29,.16), 'Slate':(.23,.35,.38), 'Stone':(.47,.49,.43), 'StoneLight':(.66,.63,.51), 'Dark':(.075,.11,.12), 'Gold':(.77,.54,.2), 'Steel':(.5,.61,.65), 'Blue':(.13,.27,.36), 'Cloth':(.54,.13,.1), 'Leaves':(.22,.36,.16), 'LeavesLight':(.36,.47,.21), 'Pine':(.15,.29,.21), 'Skin':(.65,.46,.3), 'Goblin':(.35,.48,.23), 'Flower':(.78,.58,.16), 'FlowerWhite':(.88,.83,.66), 'Lantern':(1,.61,.22), 'Cloud':(.84,.89,.86), 'Water':(.09,.42,.46)}
for name,col in colors.items():
 p=ROOT/'Art/Materials'/('M_'+name+'.mat')
 shader=meta(ROOT/'Art/Shaders'/('LakeWater.shader' if name=='Water' else 'LakesideLit.shader'),'ShaderImporter')
 emission=(.9,.38,.06) if name=='Lantern' else (0,0,0)
 p.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!21 &2100000\nMaterial:\n  serializedVersion: 8\n  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n  m_Name: M_'+name+'\n  m_Shader: '+asset(shader,4800000)+'\n  m_ValidKeywords: []\n  m_InvalidKeywords: []\n  m_LightmapFlags: 4\n  m_EnableInstancingVariants: 0\n  m_DoubleSidedGI: 0\n  m_CustomRenderQueue: -1\n  stringTagMap: {}\n  disabledShaderPasses: []\n  m_SavedProperties:\n    serializedVersion: 3\n    m_TexEnvs:\n    - _BaseMap:\n        m_Texture: {fileID: 0}\n        m_Scale: {x: 1, y: 1}\n        m_Offset: {x: 0, y: 0}\n    m_Ints: []\n    m_Floats:\n    - _Ground: '+str(int(name=='Grass'))+'\n    - _Smoothness: '+str(.7 if name=='Steel' else .18)+'\n    - _Cull: 2\n    - _Cutoff: 0.5\n    m_Colors:\n    - _BaseColor: '+vec((*col,.85 if name=='Water' else 1),'rgba')+'\n    - _EmissionColor: '+vec((*emission,1),'rgba')+'\n  m_BuildTextureStacks: []\n')
 materials[name]=meta(p,'NativeFormatImporter',2100000)
# A local sky asset uses the built-in procedural sky shader (file ID 106).
sky=ROOT/'Art/Materials/M_Sky.mat'
sky.write_text('%YAML 1.1\n%TAG !u! tag:unity3d.com,2011:\n--- !u!21 &2100000\nMaterial:\n  serializedVersion: 8\n  m_ObjectHideFlags: 0\n  m_Name: M_Sky\n  m_Shader: {fileID: 106, guid: 0000000000000000f000000000000000, type: 0}\n  m_ValidKeywords:\n  - _SUNDISK_HIGH_QUALITY\n  m_InvalidKeywords: []\n  m_LightmapFlags: 4\n  m_CustomRenderQueue: -1\n  m_SavedProperties:\n    serializedVersion: 3\n    m_TexEnvs: []\n    m_Ints: []\n    m_Floats:\n    - _SunDisk: 2\n    - _SunSize: 0.035\n    - _SunSizeConvergence: 5\n    - _AtmosphereThickness: 1\n    - _Exposure: 1.1\n    m_Colors:\n    - _SkyTint: {r: 0.43, g: 0.59, b: 0.69, a: 1}\n    - _GroundColor: {r: 0.33, g: 0.36, b: 0.27, a: 1}\n')
skyg=meta(sky,'NativeFormatImporter',2100000)
meshes={}
def mesh(name, triangles):
 vertices=[];data=b''
 for tri in triangles:
  a,b,c=tri;u=[b[i]-a[i] for i in range(3)];v=[c[i]-a[i] for i in range(3)]
  n=(u[1]*v[2]-u[2]*v[1],u[2]*v[0]-u[0]*v[2],u[0]*v[1]-u[1]*v[0]);length=math.sqrt(sum(x*x for x in n)) or 1
  n=tuple(x/length for x in n)
  for pt in tri:vertices.append(pt);data+=struct.pack('<6f',*pt,*n)
 count=len(vertices);fmt=1 if count>65535 else 0
 indices=struct.pack('<'+('I' if fmt else 'H')*count,*range(count)).hex()
 mins=[min(v[i] for v in vertices) for i in range(3)];maxs=[max(v[i] for v in vertices) for i in range(3)]
 center=tuple((a+b)/2 for a,b in zip(mins,maxs));extent=tuple((b-a)/2 for a,b in zip(mins,maxs))
 channels=''
 for j in range(14):channels+=f'    - stream: 0\n      offset: {12 if j==1 else 0}\n      format: 0\n      dimension: {3 if j<2 else 0}\n'
 p=ROOT/'Art/Meshes'/(name+'.asset')
 p.write_text(f'''%YAML 1.1
%TAG !u! tag:unity3d.com,2011:
--- !u!43 &4300000
Mesh:
  m_ObjectHideFlags: 0
  m_CorrespondingSourceObject: {{fileID: 0}}
  m_PrefabInstance: {{fileID: 0}}
  m_PrefabAsset: {{fileID: 0}}
  m_Name: {name}
  serializedVersion: 11
  m_SubMeshes:
  - serializedVersion: 2
    firstByte: 0
    indexCount: {count}
    topology: 0
    baseVertex: 0
    firstVertex: 0
    vertexCount: {count}
    localAABB:
      m_Center: {vec(center)}
      m_Extent: {vec(extent)}
  m_Shapes:
    vertices: []
    shapes: []
    channels: []
    fullWeights: []
  m_BindPose: []
  m_BoneNameHashes:
  m_RootBoneNameHash: 0
  m_BonesAABB: []
  m_VariableBoneCountWeights:
    m_Data:
  m_MeshCompression: 0
  m_IsReadable: 1
  m_KeepVertices: 1
  m_KeepIndices: 1
  m_IndexFormat: {fmt}
  m_IndexBuffer: {indices}
  m_VertexData:
    serializedVersion: 3
    m_VertexCount: {count}
    m_Channels:
{channels}    m_DataSize: {len(data)}
    _typelessdata: {data.hex()}
  m_CompressedMesh:
    m_Vertices: {{m_NumItems: 0, m_Range: 0, m_Start: 0, m_Data: , m_BitSize: 0}}
    m_UV: {{m_NumItems: 0, m_Range: 0, m_Start: 0, m_Data: , m_BitSize: 0}}
    m_Normals: {{m_NumItems: 0, m_Range: 0, m_Start: 0, m_Data: , m_BitSize: 0}}
    m_Tangents: {{m_NumItems: 0, m_Range: 0, m_Start: 0, m_Data: , m_BitSize: 0}}
    m_Weights: {{m_NumItems: 0, m_Data: , m_BitSize: 0}}
    m_NormalSigns: {{m_NumItems: 0, m_Data: , m_BitSize: 0}}
    m_TangentSigns: {{m_NumItems: 0, m_Data: , m_BitSize: 0}}
    m_FloatColors: {{m_NumItems: 0, m_Range: 0, m_Start: 0, m_Data: , m_BitSize: 0}}
    m_BoneIndices: {{m_NumItems: 0, m_Data: , m_BitSize: 0}}
    m_Triangles: {{m_NumItems: 0, m_Data: , m_BitSize: 0}}
    m_UVInfo: 0
  m_LocalAABB:
    m_Center: {vec(center)}
    m_Extent: {vec(extent)}
  m_MeshUsageFlags: 0
  m_CookingOptions: 30
  m_BakedConvexCollisionMesh:
  m_BakedTriangleCollisionMesh:
  m_MeshMetrics[0]: 1
  m_MeshMetrics[1]: 1
  m_MeshOptimizationFlags: -1
  m_StreamData:
    offset: 0
    size: 0
    path:
''')
 meshes[name]=meta(p,'NativeFormatImporter',4300000)

def height(x,z):
 r=math.hypot((x-12)/17,(z-10)/13)
 lake=-3.2*(1-min(1,r*r)) if r<1 else 0
 edge=max(0,(max(abs(x),abs(z))-39)/21)
 return lake+edge*edge*(7+3*math.sin(x*.13)*math.cos(z*.1))
tri=[];n=80
for iz in range(n):
 for ix in range(n):
  x=-60+ix*120/n;z=-60+iz*120/n;d=120/n
  a=(x,height(x,z),z);b=(x+d,height(x+d,z),z);c=(x,height(x,z+d),z+d);e=(x+d,height(x+d,z+d),z+d)
  tri.extend([(a,c,b),(b,c,e)])
mesh('Landscape',tri)
tri=[]
for i in range(64):
 a=i*math.tau/64;b=(i+1)*math.tau/64
 tri.append(((0,0,0),(17*.89*math.cos(b),0,13*.89*math.sin(b)),(17*.89*math.cos(a),0,13*.89*math.sin(a))))
mesh('LakeDisc',tri)
tri=[]
for i in range(8):
 a=i*math.tau/8;b=(i+1)*math.tau/8;p=(math.cos(a)*.5,0,math.sin(a)*.5);q=(math.cos(b)*.5,0,math.sin(b)*.5)
 tri.extend([(p,(0,1,0),q),((0,0,0),p,q)])
mesh('Cone',tri)
# Gabled roof, unit width/height/depth.
a=(-.5,0,-.5);b=(.5,0,-.5);c=(0,1,-.5);d=(-.5,0,.5);e=(.5,0,.5);f=(0,1,.5)
mesh('Gable',[(a,c,b),(d,e,f),(a,d,f),(a,f,c),(b,c,f),(b,f,e),(a,b,e),(a,e,d)])
# Angular icosahedron from a normalized golden-ratio vertex set.
t=(1+math.sqrt(5))/2
v=[(-1,t,0),(1,t,0),(-1,-t,0),(1,-t,0),(0,-1,t),(0,1,t),(0,-1,-t),(0,1,-t),(t,0,-1),(t,0,1),(-t,0,-1),(-t,0,1)]
v=[tuple(c/math.sqrt(1+t*t)*.5 for c in p) for p in v]
faces=[(0,11,5),(0,5,1),(0,1,7),(0,7,10),(0,10,11),(1,5,9),(5,11,4),(11,10,2),(10,7,6),(7,1,8),(3,9,4),(3,4,2),(3,2,6),(3,6,8),(3,8,9),(4,9,5),(2,4,11),(6,2,10),(8,6,7),(9,8,1)]
mesh('FacetedRock',[tuple(v[i] for i in f) for f in faces])

# Scene serialization.
common='  m_ObjectHideFlags: 0\n  m_CorrespondingSourceObject: {fileID: 0}\n  m_PrefabInstance: {fileID: 0}\n  m_PrefabAsset: {fileID: 0}\n'
objects=[];blocks=[];nextid=10000
class Obj:
 def __init__(self,name,parent=None,pos=(0,0,0),scale=(1,1,1),rot=(0,0,0),layer=0,tag='Untagged'):
  global nextid
  self.id=nextid;self.tr=nextid+1;nextid+=2;self.name=name;self.parent=parent;self.pos=pos;self.scale=scale;self.rot=rot;self.layer=layer;self.tag=tag;self.children=[];self.components=[self.tr];objects.append(self)
  if parent:parent.children.append(self)
 def comp(self,cls,name,fields):
  global nextid
  i=nextid;nextid+=1;self.components.append(i)
  blocks.append(f'--- !u!{cls} &{i}\n{name}:\n'+common+f'  m_GameObject: {ref(self.id)}\n'+fields);return i
 def script(self,name,fields=''):
  g=meta(ROOT/'Scripts'/(name+'.cs'),'MonoImporter')
  return self.comp(114,'MonoBehaviour',f'  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {asset(g,11500000)}\n  m_Name:\n  m_EditorClassIdentifier:\n'+fields)
 def boxcol(self,size=(1,1,1),center=(0,0,0),trigger=False):
  return self.comp(65,'BoxCollider',f'  m_Material: {{fileID: 0}}\n  m_IsTrigger: {int(trigger)}\n  m_Enabled: 1\n  serializedVersion: 3\n  m_Size: {vec(size)}\n  m_Center: {vec(center)}\n')
 def render(self,shape,mat,collide=False):
  builtin={'Cube':10202,'Sphere':10207,'Cylinder':10206,'Capsule':10208}
  m=asset(meshes[shape],4300000) if shape in meshes else '{fileID: '+str(builtin[shape])+', guid: 0000000000000000e000000000000000, type: 0}'
  self.comp(33,'MeshFilter','  m_Mesh: '+m+'\n')
  self.comp(23,'MeshRenderer',f'''  m_Enabled: 1
  m_CastShadows: {0 if mat in ['Water','Cloud','Lantern'] else 1}
  m_ReceiveShadows: 1
  m_DynamicOccludee: 1
  m_StaticShadowCaster: 0
  m_MotionVectors: 1
  m_LightProbeUsage: 1
  m_ReflectionProbeUsage: 1
  m_RayTracingMode: 2
  m_RenderingLayerMask: 1
  m_RendererPriority: 0
  m_Materials:
  - {asset(materials[mat],2100000)}
  m_StaticBatchInfo: {{firstSubMesh: 0, subMeshCount: 0}}
  m_StaticBatchRoot: {{fileID: 0}}
  m_ProbeAnchor: {{fileID: 0}}
  m_LightProbeVolumeOverride: {{fileID: 0}}
  m_ScaleInLightmap: 1
  m_ReceiveGI: 1
  m_PreserveUVs: 0
  m_IgnoreNormalsForChartDetection: 0
  m_ImportantGI: 0
  m_StitchLightmapSeams: 1
  m_SelectedEditorRenderState: 3
  m_MinimumChartSize: 4
  m_AutoUVMaxDistance: 0.5
  m_AutoUVMaxAngle: 89
  m_LightmapParameters: {{fileID: 0}}
  m_SortingLayerID: 0
  m_SortingLayer: 0
  m_SortingOrder: 0
  m_AdditionalVertexStreams: {{fileID: 0}}
''')
  if collide:
   if shape in meshes:self.comp(64,'MeshCollider',f'  m_Material: {{fileID: 0}}\n  m_IsTrigger: 0\n  m_Enabled: 1\n  serializedVersion: 5\n  m_Convex: 0\n  m_CookingOptions: 30\n  m_Mesh: {m}\n')
   else:self.boxcol()
  return self

def prop(name,parent,pos,scale,mat='Wood',shape='Cube',rot=(0,0,0),solid=False):return Obj(name,parent,pos,scale,rot).render(shape,mat,solid)
# Reuse the checked-in Unity camera/light formats for compatibility with this project.
sample=Path('Assets/Scenes/SampleScene.unity').read_text()
def template(name):
 return re.search(r'--- !u!\d+ &\d+\n'+name+r':\n(.*?)(?=--- !u!|\Z)',sample,re.S)[1].split('  m_GameObject:')[1].split('\n',1)[1]
def light(name,parent,pos,kind=2,color=(1,.67,.31),intensity=2.2,rot=(0,0,0),range_=7):
 o=Obj(name,parent,pos,rot=rot);s=template('Light')
 s=re.sub(r'  m_Type: .*',f'  m_Type: {kind}',s);s=re.sub(r'  m_Color: .*','  m_Color: '+vec((*color,1),'rgba'),s);s=re.sub(r'  m_Intensity: .*',f'  m_Intensity: {intensity}',s);s=re.sub(r'  m_Range: .*',f'  m_Range: {range_}',s)
 if kind==2:s=s.replace('    m_Type: 2','    m_Type: 0')
 return o,o.comp(108,'Light',s)
world=Obj('Lakeside Village')
env=Obj('01 Environment',world)
ground=Obj('01 Landscape and Lake Basin',env).render('Landscape','Grass',True)
watergroup=Obj('02 Lake and Swimming',env)
water=prop('Lake Surface',watergroup,(12,-.88,10),(1,1,1),'Water','LakeDisc');water.layer=2;water.script('WaterScroll','  scrollSpeedX: 0.035\n  scrollSpeedY: 0.017\n')
trigger=Obj('Swimming Volume',watergroup,(12,-2.05,10),layer=2);trigger.boxcol((29,2.3,22),trigger=True);trigger.script('WaterVolume')
# A mesh-shaped exclusion prevents navigation into the basin while leaving the bridge walkable.
# NavMesh source filters are configured in the runtime on this serialized landscape.
village=Obj('03 Village Buildings',env)
paths=Obj('04 Stone Paths and Courtyard',env)
bridge=Obj('05 Timber Bridge and Dock',env)
forest=Obj('06 Trees and Undergrowth',env)
rocks=Obj('07 Rocks and Mountains',env)
details=Obj('08 Village Props',env)
lamps=[]
def lantern(parent,x,z,y=0):
 root=Obj('Lantern Post',parent,(x,y,z));prop('Oak Post',root,(0,1.6,0),(.16,3.2,.16),solid=True)
 prop('Bracket',root,(.26,3.12,0),(.68,.12,.12));prop('Lamp Frame',root,(.53,2.72,0),(.4,.64,.4),'Dark')
 prop('Warm Glass',root,(.53,2.74,-.205),(.27,.42,.015),'Lantern');prop('Warm Glass Back',root,(.53,2.74,.205),(.27,.42,.015),'Lantern')
 prop('Lamp Cap',root,(.53,3.08,0),(.58,.25,.58),'Dark','Cone')
 lo,li=light('Lantern Light',root,(.53,2.7,0),intensity=.1);lamps.append((lo,li))
def house(name,x,z,yaw,w=6,d=5,tall=3.1,roof='Roof'):
 root=Obj(name,village,(x,height(x,z),z),rot=(0,yaw,0))
 prop('Stone Foundation',root,(0,.16,0),(w+.35,.32,d+.35),'Stone',solid=True)
 prop('Plaster Walls',root,(0,tall*.5+.32,0),(w,tall,d),'PlasterCream' if roof=='Slate' else 'Plaster',solid=True)
 prop('Gabled Roof',root,(0,tall+.32,0),(w+.95,2.1,d+1),'Roof' if roof=='Roof' else roof,'Gable',solid=True)
 for xx in [-w/2+.1,w/2-.1]:
  for zz in [-d/2-.01,d/2+.01]:prop('Corner Timber',root,(xx,tall/2+.32,zz),(.18,tall,.18))
 for yy in [.6,tall+.25]:
  prop('Front Beam',root,(0,yy,-d/2-.025),(w,.16,.13));prop('Back Beam',root,(0,yy,d/2+.025),(w,.16,.13))
 prop('Oak Door',root,(0,1.38,-d/2-.06),(1.1,2.05,.12),'Wood')
 for xx in [-.63,.63]:prop('Door Frame',root,(xx,1.4,-d/2-.13),(.13,2.3,.16),'WoodLight')
 prop('Door Header',root,(0,2.55,-d/2-.13),(1.4,.18,.2),'WoodLight');prop('Brass Handle',root,(.35,1.35,-d/2-.15),(.06,.11,.05),'Gold','Sphere')
 for xx in [-w*.32,w*.32]:
  prop('Window Recess',root,(xx,1.85,-d/2-.035),(1.1,1.25,.1),'Dark')
  prop('Window Glass',root,(xx,1.85,-d/2-.1),(.84,1,.025),'Lantern')
  prop('Window Cross',root,(xx,1.85,-d/2-.12),(.08,1.1,.07),'WoodLight');prop('Window Cross',root,(xx,1.85,-d/2-.12),(1,.08,.07),'WoodLight')
  for dx in [-.62,.62]:prop('Shutter',root,(xx+dx,1.85,-d/2-.09),(.22,1.3,.12),'Blue')
  prop('Window Sill',root,(xx,1.18,-d/2-.2),(1.5,.14,.38),'WoodLight')
 prop('Chimney',root,(w*.26,tall+1.65,.65),(.72,2,.76),'Stone',solid=True)
 prop('Chimney Cap',root,(w*.26,tall+2.67,.65),(.92,.15,.96),'StoneLight')
 # Individual ridge tiles and diagonal roof battens break up the large roof faces.
 for zz in range(int(d/.6)+2):prop('Ridge Tile',root,(0,tall+2.45,-d/2-.2+zz*.6),(.4,.16,.55),'RoofLight')
 for side in [-1,1]:
  for k in range(5):
   xx=side*(k+.5)*(w+.95)/10;yy=tall+2.42-abs(xx)*2.1/((w+.95)/2)
   prop('Roof Tile Course',root,(xx,yy,0),((w+.95)/10,.035,d+.98),'RoofLight' if roof=='Roof' else 'Slate',rot=(0,0,-side*math.degrees(math.atan(2.1/((w+.95)/2)))))
 for side in [-1,1]:
  for zz in [-d*.25,d*.25]:
   prop('Side Window',root,(side*(w/2+.04),1.8,zz),(.08,1.05,.95),'Dark')
   prop('Side Shutter',root,(side*(w/2+.1),1.8,zz),(.1,.95,.32),'Blue')
  prop('Side Mid Beam',root,(side*(w/2+.07),1.1,0),(.13,.12,d),'Wood')
  prop('Diagonal Timber Brace',root,(side*(w/2+.08),2.1,0),(.12,1.4,.14),'Wood',rot=(38,0,0))
 prop('Entry Step',root,(0,.18,-d/2-.5),(1.8,.36,.7),'StoneLight',solid=True)
 for k in range(4):
  prop('Entrance Paving',root,(0,.03,-d/2-1.25-k*.9),(1.5,.08,.8),'StoneLight')
 prop('Hanging Sign Bracket',root,(1,2.65,-d/2-.55),(.1,.1,1.15),'Wood')
 prop('Shop Sign',root,(1,2.21,-d/2-1),(.72,.62,.1),'WoodLight')
 prop('Sign Emblem',root,(1,2.21,-d/2-1.08),(.25,.34,.04),'Gold','FacetedRock')
 return root
house('Lakeside Inn',-15,5,-90,8,6,3.4)
house('Fisherman Cottage',-9,22,0,5.5,4.8,2.7,'Slate')
house('Workshop',-25,-8,35,6.3,5,3)
house('Watchman House',30,29,30,5.8,5,3,'Slate')
house('Storage Shed',-24,22,-90,4,4,2.3)
# Main paths: staggered individually editable slabs and a small circular courtyard.
for i in range(34):
 z=-29+i*1.7;x=-6+math.sin(i*.16)*1.2
 for j in range(3):
  xx=x+(j-1)*1.08;zz=z+(j%2)*.25
  prop('Path Slab',paths,(xx,height(xx,zz)+.04,zz),(1.02,.12,1.56),'StoneLight' if random.random()<.5 else 'Stone',rot=(0,random.uniform(-4,4),0))
for i in range(19):
 x=-6+i*1.25;z=-7
 for j in [-1,0,1]:prop('Bridge Approach Slab',paths,(x,.06,z+j*1.0),(1.18,.12,.95),'StoneLight')
for i in range(22):
 a=i*math.tau/22
 prop('Courtyard Paving',paths,(-8+math.cos(a)*4,.035,-16+math.sin(a)*4),(.85,.1,1.4),'Stone',rot=(0,-math.degrees(a),0))
# Well centerpiece.
well=Obj('Village Well',details,(-8,0,-16))
for i in range(12):
 a=i*math.tau/12;prop('Well Stone',well,(math.cos(a)*1.05,.55,math.sin(a)*1.05),(.45,1.1,.53),'StoneLight',rot=(0,-math.degrees(a),0),solid=True)
for xx in [-1.3,1.3]:prop('Well Support',well,(xx,1.6,0),(.18,3.2,.18),solid=True)
prop('Well Roof',well,(0,3,0),(3.4,1.1,2.2),'Roof','Gable');prop('Well Rope',well,(0,1.9,0),(.025,2.1,.025),'WoodLight');prop('Winch',well,(0,2.4,0),(2.8,.15,.15),'Wood')
# Bridge spans the lake north/south; dense planks create a continuous collision deck.
for i in range(56):
 z=-7+i*.6;y=.25+math.sin(i/55*math.pi)*.45
 prop('Deck Plank',bridge,(12,y,z),(3.8,.18,.57),'Planks' if i%3 else 'WoodLight',solid=True)
for side in [-1,1]:
 for i in range(12):
  z=-7+i*3;y=.25+math.sin(i/11*math.pi)*.45
  prop('Bridge Post',bridge,(12+side*1.85,y+.8,z),(.19,1.6,.19),'Wood',solid=True)
  prop('Post Cap',bridge,(12+side*1.85,y+1.65,z),(.32,.14,.32),'Gold')
 for i in range(11):
  z=-5.5+i*3;y=.25+math.sin((i+.5)/11*math.pi)*.45
  prop('Bridge Handrail',bridge,(12+side*1.85,y+1.45,z),(.15,.15,3.08),'WoodLight',solid=True)
  prop('Lower Rail',bridge,(12+side*1.85,y+.65,z),(.1,.1,3.08),'Wood')
for z in [-6,26]:
 for x in [9.4,14.6]:lantern(bridge,x,z)
# Dock and flat-bottomed boat near the western shore.
for i in range(12):prop('Dock Plank',bridge,(-1+i*.48,.12,8),( .46,.18,3),'Planks',solid=True)
for x in [-1,3.7]:
 for z in [6.7,9.3]:prop('Dock Piling',bridge,(x,-.55,z),(.23,2.1,.23),'Wood','Cylinder',solid=True)
boat=Obj('Fishing Boat',details,(4.5,-.68,12),rot=(0,-16,0))
prop('Hull Floor',boat,(0,0,0),(1.2,.16,3.8),'WoodLight')
for side in [-1,1]:prop('Hull Side',boat,(side*.63,.21,0),(.12,.6,3.8),'Planks',rot=(0,0,-side*12))
for zz in [-1.85,1.85]:prop('Hull End',boat,(0,.19,zz),(1.3,.55,.1),'Planks')
for zz in [-1,0,1]:prop('Bench',boat,(0,.35,zz),(1.28,.12,.3),'Wood')
prop('Oar',boat,(.78,.6,0),(.055,.055,3.4),'WoodLight',rot=(0,24,0));prop('Oar Blade',boat,(1.45,.6,1.4),(.32,.05,.55),'WoodLight',rot=(0,24,0))
# Forest edges and small mixed groves, keeping entrances and paths clear.
for i in range(70):
 a=random.random()*math.tau;r=random.uniform(34,53);x=math.cos(a)*r;z=math.sin(a)*r
 if (25<x<36 and 22<z<36) or (-31<x<-17 and 15<z<29):continue
 tree=Obj(f'Tree {i+1:02}',forest,(x,height(x,z),z),rot=(0,random.randrange(360),0));size=random.uniform(.8,1.3)
 prop('Trunk',tree,(0,1.8*size,0),(.5*size,1.8*size,.5*size),'Wood','Cylinder',solid=True)
 if i%3==0:
  for k in range(3):prop('Pine Crown',tree,(0,(2.5+k*1.25)*size,0),((4.3-k)*size,2.6*size,(4.3-k)*size),'Pine','Cone')
 else:
  for k in range(4):prop('Leaf Crown',tree,((k%2-.5)*1.4*size,(3.6+k//2)*size,(k//2-.5)*1.2*size),(3*size,2.8*size,2.9*size),'Leaves' if k%2 else 'LeavesLight','FacetedRock')
for i in range(36):
 a=random.uniform(0,math.tau);x=12+math.cos(a)*random.uniform(17,21);z=10+math.sin(a)*random.uniform(13,17)
 if abs(x-12)<3 or (-3<x<5 and 5<z<11):continue
 prop('Shore Boulder',rocks,(x,height(x,z)+.3,z),(random.uniform(.6,1.8),random.uniform(.5,1.2),random.uniform(.6,1.5)),'Stone','FacetedRock',rot=(0,random.randrange(360),0),solid=True)
for i in range(14):
 a=i*math.tau/14;x=math.cos(a)*68;z=math.sin(a)*68
 prop('Distant Mountain',rocks,(x,1,z),(random.uniform(22,34),random.uniform(16,27),random.uniform(22,32)),'Slate' if i%2 else 'Stone','FacetedRock',rot=(0,i*37,0))
for i in range(120):
 x=random.uniform(-36,36);z=random.uniform(-33,37)
 if math.hypot((x-12)/17,(z-10)/13)<1.1 or abs(x+6)<4 or abs(z+7)<2 or (x<-10 and z>-13):continue
 cluster=Obj('Wildflowers',forest,(x,height(x,z),z))
 for k in range(3):
  xx=random.uniform(-.4,.4);zz=random.uniform(-.4,.4);h=random.uniform(.22,.5)
  prop('Stem',cluster,(xx,h*.5,zz),(.025,h,.025),'Leaves')
  prop('Flower',cluster,(xx,h,zz),(.16,.07,.16),'FlowerWhite' if i%3 else 'Flower','FacetedRock')
# Shore reeds, crates, barrels, benches, fences, market stall.
for i in range(42):
 a=random.uniform(0,math.tau);x=12+math.cos(a)*16.5;z=10+math.sin(a)*12.5
 if abs(x-12)<3 or (-2<x<7 and 5<z<15):continue
 for k in range(3):
  xx=x+random.uniform(-.22,.22);zz=z+random.uniform(-.22,.22);hh=random.uniform(.65,1.15);y=height(xx,zz)
  prop('Reed',forest,(xx,y+hh*.5,zz),(.055,hh,.055),'Leaves',rot=(random.uniform(-8,8),0,random.uniform(-8,8)))
  prop('Reed Head',forest,(xx,y+hh,zz),(.13,.25,.13),'WoodLight','Cylinder')
def barrel(x,z,parent=details):
 root=Obj('Oak Barrel',parent,(x,height(x,z),z));prop('Barrel Body',root,(0,.55,0),(.75,.55,.75),'Planks','Cylinder',solid=True)
 for y in [.13,.95]:prop('Iron Hoop',root,(0,y,0),(.8,.045,.8),'Dark','Cylinder')
 prop('Lid',root,(0,1.12,0),(.73,.025,.73),'WoodLight','Cylinder')
for x,z in [(-12,0),(-13,0),(-20,-4),(-22,18),(1,6),(2,6),(29,25)]:barrel(x,z)
for x,z in [(-20,-5),(-22,-4),(-10,25),(-26,17),(0,7)]:
 root=Obj('Supply Crate',details,(x,height(x,z)+.48,z));prop('Crate',root,(0,0,0),(.95,.95,.95),'Planks',solid=True)
 for yy in [-.37,.37]:prop('Crate Brace',root,(0,yy,-.48),(1,.1,.08),'WoodLight')
 prop('Diagonal Brace',root,(0,0,-.5),(1.2,.1,.09),'WoodLight',rot=(0,0,43))
for x,z in [(-10,-22),(-4,18),(32,19)]:
 r=Obj('Bench',details,(x,0,z));prop('Seat',r,(0,.55,0),(2.7,.16,.65),'WoodLight',solid=True)
 prop('Back',r,(0,1.06,.32),(2.7,.65,.12),'Planks')
 for xx in [-.95,.95]:prop('Leg',r,(xx,.27,0),(.15,.55,.55),'Wood')
for i in range(12):
 x=-31+i*1.8;z=-15
 prop('Fence Post',details,(x,.62,z),(.16,1.25,.16),'Wood',solid=True)
 if i<11:
  for y in [.45,.9]:prop('Fence Rail',details,(x+.9,y,z),(1.85,.12,.12),'WoodLight',solid=True)
market=Obj('Market Stall',details,(-16,0,-18))
for x in [-1.6,1.6]:
 for z in [-1,1]:prop('Awning Post',market,(x,1.5,z),(.15,3,.15),'Wood',solid=True)
for i in range(8):prop('Striped Awning',market,(-1.75+i*.5,3,0),(.49,.12,2.65),'Cloth' if i%2 else 'PlasterCream',rot=(0,0,3))
prop('Counter',market,(0,.95,0),(3.5,.18,1.5),'Planks',solid=True)
for i in range(10):prop('Produce',market,(random.uniform(-1.4,1.4),1.12,random.uniform(-.5,.5)),(.22,.22,.22),'Flower' if i%2 else 'LeavesLight','Sphere')
for x,z in [(-4,-28),(-4,-12),(-3,18),(-10,7),(30,23)]:lantern(details,x,z)

# Additional gardens, a guard encampment, and short grass keep each area distinct.
gardens=Obj('09 Gardens and Guard Camp',env)
for x,z in [(-20,10),(-16,24)]:
 bed=Obj('Vegetable Garden',gardens,(x,0,z))
 prop('Garden Soil',bed,(0,.025,0),(4,.08,3),'Wood')
 for side in [-1,1]:prop('Garden Border',bed,(side*2,.12,0),(.16,.25,3.3),'WoodLight')
 for i in range(4):
  for j in range(3):
   prop('Cabbage',bed,(-1.4+i*.9,.2,-.95+j*.9),(.48,.38,.48),'LeavesLight','FacetedRock')
for x,z in [(31,-15),(36,-10)]:
 tent=Obj('Guard Tent',gardens,(x,0,z),rot=(0,-30,0))
 prop('Canvas Tent',tent,(0,.15,0),(3.2,2.6,3.8),'Cloth','Gable',solid=True)
 prop('Tent Entrance',tent,(0,1.2,-1.91),(.95,1.8,.02),'Dark')
 for side in [-1,1]:
  prop('Tent Stake',tent,(side*2,.12,-1.6),(.09,.3,.09),'Wood')
  prop('Guy Rope',tent,(side*1.7,.53,-1.6),(.025,1.3,.025),'WoodLight',rot=(0,0,side*35))
fire=Obj('Stone Firepit',gardens,(30,0,-9))
for i in range(10):
 a=i*math.tau/10;prop('Firepit Stone',fire,(math.cos(a)*.8,.13,math.sin(a)*.8),(.35,.3,.35),'Stone','FacetedRock')
for i in range(3):prop('Charred Log',fire,(0,.16,0),(.18,.18,1.2),'Dark',rot=(0,i*60,0))
for x in [29,32]:
 target=Obj('Training Target',gardens,(x,0,-4))
 prop('Target Post',target,(0,1,0),(.12,2,.12),'Wood',solid=True)
 prop('Target',target,(0,1.8,0),(1.1,1.1,.12),'WoodLight','FacetedRock')
 prop('Bullseye',target,(0,1.8,-.09),(.35,.35,.03),'Cloth','Sphere')
for i in range(150):
 x=random.uniform(-37,37);z=random.uniform(-34,38)
 if math.hypot((x-12)/17,(z-10)/13)<1.1 or abs(x+6)<4 or abs(z+7)<2 or (x<-10 and z>-13) or (x>26 and z<-2):continue
 for k in range(3):
  prop('Short Grass',forest,(x+k*.09,height(x,z),z),( .16,random.uniform(.15,.35),.13),'Leaves' if i%2 else 'LeavesLight','Cone',rot=(0,random.uniform(0,360),random.uniform(-15,15)))

# Actors with real modeled silhouettes and animation pivots.
actors=Obj('02 Characters',world)
def character(name,x,z,enemy=False):
 root=Obj(name,actors,(x,height(x,z)+.06,z),rot=(0,180 if enemy else 20,0),tag='Untagged' if enemy else 'Player')
 if enemy:
  root.comp(136,'CapsuleCollider','  m_Material: {fileID: 0}\n  m_IsTrigger: 0\n  m_Enabled: 1\n  serializedVersion: 2\n  m_Radius: 0.43\n  m_Height: 2.05\n  m_Direction: 1\n  m_Center: {x: 0, y: 1, z: 0}\n')
  root.comp(195,'NavMeshAgent','  m_Enabled: 0\n  m_AgentTypeID: 0\n  m_Radius: 0.43\n  m_Speed: 3.5\n  m_Acceleration: 12\n  avoidancePriority: 50\n  m_AngularSpeed: 720\n  m_StoppingDistance: 1.3\n  m_AutoTraverseOffMeshLink: 0\n  m_AutoBraking: 1\n  m_AutoRepath: 1\n  m_Height: 2\n  m_BaseOffset: 0\n  m_WalkableMask: 4294967295\n  m_ObstacleAvoidanceType: 4\n')
  root.script('EnemyHealth','  maxHealth: 100\n  currentHealth: 100\n  destroyAfterDeath: 1\n  destroyDelay: 2\n')
  root.script('EnemyController',f'  playerTransform: {ref(player.tr)}\n  chaseRange: 12\n  viewDistance: 17\n  viewAngle: 170\n  obstacleMask:\n    serializedVersion: 2\n    m_Bits: 4294967291\n')
 else:
  root.comp(143,'CharacterController','  m_Material: {fileID: 0}\n  m_IsTrigger: 0\n  m_Enabled: 1\n  serializedVersion: 3\n  m_Height: 2\n  m_Radius: 0.43\n  m_SlopeLimit: 50\n  m_StepOffset: 0.35\n  m_SkinWidth: 0.08\n  m_MinMoveDistance: 0\n  m_Center: {x: 0, y: 1, z: 0}\n')
  root.script('PlayerController');root.script('PlayerCombat')
 vis=Obj('Visual',root);skin='Goblin' if enemy else 'Skin';armor='Wood' if enemy else 'Steel'
 prop('Torso',vis,(0,1.22,0),(.65,.66,.38),'Cloth' if enemy else 'Blue','Cube')
 prop('Breastplate',vis,(0,1.29,.17),(.58,.53,.13),armor,'FacetedRock');prop('Belt',vis,(0,.93,0),(.68,.1,.4),'Dark');prop('Buckle',vis,(0,.93,.23),(.14,.13,.05),'Gold')
 prop('Head',vis,(0,1.84,0),(.56,.57,.49),skin,'FacetedRock')
 if enemy:
  for side in [-1,1]:prop('Pointed Ear',vis,(side*.32,1.9,0),(.32,.2,.15),skin,'Cone',rot=(0,0,-side*75))
  prop('Nose',vis,(0,1.78,.28),(.17,.2,.16),skin,'FacetedRock')
 else:
  prop('Helmet',vis,(0,2.03,-.03),(.61,.34,.57),'Steel','FacetedRock');prop('Visor',vis,(0,1.88,.255),(.45,.14,.03),'Dark')
  prop('Helmet Crest',vis,(0,2.21,-.02),(.1,.27,.42),'Cloth','FacetedRock')
 for side in [-1,1]:prop('Eye',vis,(side*.105,1.9,.255),(.055,.045,.025),'FlowerWhite' if enemy else 'Dark','Sphere')
 joints=[]
 for side in [-1,1]:
  arm=Obj('Left Arm' if side==-1 else 'Right Arm',vis,(side*.44,1.5,0));joints.append(arm)
  prop('Shoulder',arm,(0,0,0),(.36,.29,.4),armor,'FacetedRock')
  prop('Arm',arm,(0,-.28,0),(.21,.56,.22),skin,'Cube');prop('Gauntlet',arm,(0,-.55,0),(.24,.23,.26),armor,'Cube')
  leg=Obj('Left Leg' if side==-1 else 'Right Leg',vis,(side*.19,.89,0));joints.append(leg)
  prop('Leg',leg,(0,-.33,0),(.24,.66,.24),'Dark');prop('Boot',leg,(0,-.75,.08),(.29,.24,.45),'Wood' if enemy else 'Steel')
 right=joints[2]
 weapon=Obj('Sword',right,(0,-.58,.15),rot=(-18,0,0))
 prop('Grip',weapon,(0,0,0),(.095,.22,.1),'Wood');prop('Crossguard',weapon,(0,.16,0),(.36,.07,.09),'Gold')
 prop('Blade',weapon,(0,.61,0),(.115,.84,.065),'Steel');prop('Blade Tip',weapon,(0,1.06,0),(.115,.15,.065),'Steel','Cone');prop('Pommel',weapon,(0,-.14,0),(.14,.12,.14),'Gold','FacetedRock')
 left=joints[0];shield=Obj('Shield',left,(-.12,-.35,.13),rot=(0,-22,0))
 prop('Shield Rim',shield,(0,0,0),(.1,.85,.63),'Gold','FacetedRock');prop('Shield Face',shield,(-.06,0,0),(.1,.73,.52),'Wood' if enemy else 'Blue','FacetedRock')
 prop('Shield Crest',shield,(-.12,0,0),(.035,.34,.09),'Gold')
 if not enemy:prop('Cape',vis,(0,1.04,-.27),(.56,.91,.07),'Cloth',rot=(8,0,0))
 root.script('CharacterPose',f'  leftArm: {ref(joints[0].tr)}\n  rightArm: {ref(joints[2].tr)}\n  leftLeg: {ref(joints[1].tr)}\n  rightLeg: {ref(joints[3].tr)}\n  body: {ref(vis.tr)}\n')
 return root
player=character('Player - Village Knight',-6,-24)
for name,x,z in [('Enemy - Bridge Guard',20,-9),('Enemy - Shore Scout',31,13),('Enemy - Forest Scout',-1,28)]:character(name,x,z,True)
# Lighting, weather and camera are serialized too.
systems=Obj('03 Time and Weather',world)
suno,sunid=light('Sun',systems,(0,20,0),kind=1,color=(1,.9,.73),intensity=1.25,rot=(42,-35,0))
clock=Obj('Day Night Cycle',systems);clockid=clock.script('TimeController',f'  timeMultiplier: 360\n  startHour: 9\n  sunLight: {ref(sunid)}\n  sunriseHour: 6\n  sunsetHour: 18\n  skyboxMaterial: {asset(skyg,2100000)}\n')
weather=Obj('Weather - Sunny Rain Fog',systems);weather.script('WeatherManager','  currentWeather: 0\n  enableRandomWeather: 1\n  weatherChangeInterval: 45\n')
clouds=Obj('Drifting Clouds',systems)
for i in range(8):
 root=Obj('Cloud '+str(i+1),clouds,(random.uniform(-55,55),random.uniform(24,31),random.uniform(-45,45)))
 root.script('CloudDrift','  speed: 0.6\n  wrapExtent: 75\n')
 for j in range(4):prop('Cloud Puff',root,((j-1.5)*2,random.uniform(-.25,.5),random.uniform(-.6,.6)),(4.3,1.7,2.8),'Cloud','FacetedRock')
for lo,li in lamps:lo.script('NightLantern',f'  lamp: {ref(li)}\n  clock: {ref(clockid)}\n  brightness: 2.2\n')
cameras=Obj('04 Camera and Interface',world)
cam=Obj('Main Camera',cameras,(-12,5,-32),rot=(22,35,0),tag='MainCamera')
cs=template('Camera').replace('far clip plane: 1000','far clip plane: 260').replace('near clip plane: 0.3','near clip plane: 0.15').replace('field of view: 60','field of view: 55')
camid=cam.comp(20,'Camera',cs);cam.comp(81,'AudioListener','  m_Enabled: 1\n')
cam.script('ThirdPersonCamera',f'  target: {ref(player.tr)}\n  distance: 8\n  yaw: 35\n  pitch: 22\n')
# URP camera settings and volume are copied from the project's own URP sample.
cam.comp(114,'MonoBehaviour',template('MonoBehaviour').replace('  m_RenderPostProcessing: 1','  m_RenderPostProcessing: 1'))
vol=Obj('Global Color and Bloom',cameras)
volblock=re.search(r'--- !u!114 &\d+\nMonoBehaviour:\n(.*?guid: 172515602e62fb746b5d573b38a5fe58.*?)(?=--- !u!|\Z)',sample,re.S)
# The sample has a Volume component with this stable package GUID.
profileg=meta(Path('Assets/Settings/SampleSceneProfile.asset'),'NativeFormatImporter')
vol.comp(114,'MonoBehaviour',f'  m_Enabled: 1\n  m_EditorHideFlags: 0\n  m_Script: {{fileID: 11500000, guid: 172515602e62fb746b5d573b38a5fe58, type: 3}}\n  m_Name:\n  m_EditorClassIdentifier:\n  m_IsGlobal: 1\n  priority: 0\n  blendDistance: 0\n  weight: 1\n  sharedProfile: {asset(profileg,11400000)}\n')
hud=Obj('Exploration HUD',cameras);hud.script('DemoHUD')
world.script('LakesideSceneRuntime',f'  environment: {ref(env.id)}\n  mainCamera: {ref(camid)}\n  sun: {ref(sunid)}\n  clock: {ref(clockid)}\n  lakeVolume: {ref(trigger.id)}\n  skyMaterial: {asset(skyg,2100000)}\n')
header=sample.split('--- !u!1 &')[0]
header=header.replace('  m_Fog: 0','  m_Fog: 1').replace('  m_FogDensity: 0.01','  m_FogDensity: 0.004').replace('  m_AmbientMode: 0','  m_AmbientMode: 3').replace('  m_EnableBakedLightmaps: 1','  m_EnableBakedLightmaps: 0')
header=re.sub(r'  m_SkyboxMaterial: .*','  m_SkyboxMaterial: '+asset(skyg,2100000),header);header=header.replace('  m_Sun: {fileID: 0}','  m_Sun: '+ref(sunid))
header=re.sub(r'  m_AmbientSkyColor: .*','  m_AmbientSkyColor: {r: 0.5, g: 0.56, b: 0.53, a: 1}',header)
header=re.sub(r'  m_LightingDataAsset: .*','  m_LightingDataAsset: {fileID: 0}',header)
def quat(rot):
 # Unity's ZXY rotation order.
 x,y,z=[math.radians(a)/2 for a in rot];sx,cx=math.sin(x),math.cos(x);sy,cy=math.sin(y),math.cos(y);sz,cz=math.sin(z),math.cos(z)
 return (cy*sx*cz+sy*cx*sz,sy*cx*cz-cy*sx*sz,cy*cx*sz-sy*sx*cz,cy*cx*cz+sy*sx*sz)
scene=[header]
for o in objects:
 scene.append(f'--- !u!1 &{o.id}\nGameObject:\n'+common+'  serializedVersion: 6\n  m_Component:\n'+''.join('  - component: '+ref(c)+'\n' for c in o.components)+f'  m_Layer: {o.layer}\n  m_Name: {o.name}\n  m_TagString: {o.tag}\n  m_Icon: {{fileID: 0}}\n  m_NavMeshLayer: 0\n  m_StaticEditorFlags: {4 if o.parent and o.components and o.layer != 2 and any(o is c or o.parent is c for c in [ground,village,paths,bridge,forest,rocks,details]) else 0}\n  m_IsActive: 1\n')
 scene.append(f'--- !u!4 &{o.tr}\nTransform:\n'+common+f'  m_GameObject: {ref(o.id)}\n  serializedVersion: 2\n  m_LocalRotation: {vec(quat(o.rot),"xyzw")}\n  m_LocalPosition: {vec(o.pos)}\n  m_LocalScale: {vec(o.scale)}\n  m_ConstrainProportionsScale: 0\n'+('  m_Children:\n'+''.join('  - '+ref(c.tr)+'\n' for c in o.children) if o.children else '  m_Children: []\n')+'  m_Father: '+ref(o.parent.tr if o.parent else 0)+f'\n  m_LocalEulerAnglesHint: {vec(o.rot)}\n')
scene.extend(blocks);scene.append('--- !u!1660057539 &9223372036854775807\nSceneRoots:\n  m_ObjectHideFlags: 0\n  m_Roots:\n  - '+ref(world.tr)+'\n')
(ROOT/'Scenes/Chapter14_Demo.unity').write_text('\n'.join(line.rstrip() for line in ''.join(scene).splitlines())+'\n')
print(json.dumps({'objects':len(objects),'components':len(blocks),'materials':len(materials),'meshes':len(meshes),'scene_bytes':sum(len(x) for x in scene)}))
