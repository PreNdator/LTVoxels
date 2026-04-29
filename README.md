# LTVoxels

LTVoxels — Unity-проект для загрузки, хранения, отрисовки, редактирования и разбиения voxel chunks. Основной код хранит данные в `NativeArray`, строит Unity `Mesh` через Jobs/Burst, поддерживает цвета и material submeshes, а также содержит демо-сцены для загрузки, редактирования и splitting по связанным компонентам.

## Возможности

- Хранение voxel chunk через `VoxelChunk` и `DamagableChunk`.
- Генерация мешей через `IVoxelMeshBuilder` и `VoxelMeshBuilder`.
- Jobs/Burst-логика для видимых граней, генерации вершин, vertex colors и масок.
- Импорт MagicaVoxel `.vox`, MagicaVoxel ASCII `.ply` и внутреннего формата `.voxch`.
- Синхронный и асинхронный пайплайны загрузки чанков.
- Byte sources для импортированных ассетов, StreamingAssets, Addressables, файловой системы и web requests.
- Runtime-редактирование вокселей через переиспользуемые маски: sphere, cube, fill, one-voxel, combined masks и array-copy masks.
- Appliers для изменения материала, цвета, voxel type, damage multiplier и разрушения вокселей.
- Разбиение chunk на связанные компоненты с режимами соседства 6, 18 и 26.
- Zenject installers для drawing, serialization, editing, splitting и scene loading.
- Edit Mode тесты для сериализации, построения чанков, масок, splitting и span helpers.

## Требования

- Unity `6000.0.67f1`.
- Universal Render Pipeline.
- Unity packages восстанавливаются из `Packages/manifest.json`: Addressables, Test Framework, Memory Profiler, UniTask, NuGetForUnity и другие зависимости.
- Zenject лежит в проекте: `Assets/Plugins/Zenject`.
- NuGet-зависимости для тестов описаны в `Assets/Plugins/NuGet/packages.config`.

## Быстрый старт

1. Склонируйте репозиторий.
2. Откройте папку проекта через Unity Hub в Unity `6000.0.67f1`.
3. Дождитесь восстановления зависимостей через Unity Package Manager и NuGetForUnity.
4. Откройте одну из демо-сцен из `Assets/Scenes`.
5. Нажмите Play.

## Демо-сцены

| Сцена | Назначение |
| --- | --- |
| `Assets/Scenes/Main.unity` | Стартовая сцена с переходами к демо. |
| `Assets/Scenes/ChunkLoad.unity` | Загрузка voxel assets и построение chunk meshes. |
| `Assets/Scenes/ChunkEditing.unity` | Runtime-редактирование чанка движущейся sphere mask. |
| `Assets/Scenes/ChunkSplit.unity` | Разбиение чанка на связанные части и обратное объединение. |

Эти же сцены зарегистрированы в `ProjectSettings/EditorBuildSettings.asset`.

## Структура проекта

```text
Assets/
  Art/                         Materials, shaders и примерные voxel assets
  Scenes/                      Демо-сцены
  Scripts/
    Core/                      Основная voxel-логика без привязки к MonoBehaviour
      Voxel/Data/              VoxelChunk, voxel types, indexing, neighbors
      Voxel/Drawing/           Mesh generation, jobs, chunk-space strategies
      Voxel/Editing/           Masks и editing/breaking modifiers
      Voxel/Serialization/     Importers, exporters, byte sources, providers
      Voxel/Splitting/         Splitting по связанным компонентам
    Runtime/                   Unity behaviours, examples и Zenject installers
    Editor/                    Asset importers и custom drawers
    Tests/EditModeTests/       NUnit Edit Mode tests
  StreamingAssets/Examples/    Примерные runtime voxel files
Packages/                      Unity package manifest и lock file
ProjectSettings/               Настройки Unity-проекта
```

## Поддерживаемые voxel inputs

### `.vox`

MagicaVoxel `.vox` импортируется через `MagicaVoxelVoxImporter`. Импортер поддерживает VOX version `150`, читает первую модель, переводит координаты из Z-up MagicaVoxel в Y-up Unity и использует palette как voxel colors, если она есть в файле.

### `.ply`

ASCII PLY point cloud в формате экспорта MagicaVoxel импортируется через `PlyPointCloudImporter`. Импортер читает integer positions и RGB colors, после чего переводит координаты в Y-up пространство Unity.

### `.voxch`

`.voxch` — внутренний формат chunk, реализованный в `VoxchImportExport`. Он хранит размеры чанка, batch size, voxel types, material IDs и опциональные colors.

### Импорт в Unity

Если добавить `.vox` или `.ply` в папку `Assets`, scripted importer из `Assets/Scripts/Editor/VoxelBytesImporter.cs` создаст `VoxelBytesAsset`. Runtime loaders могут использовать этот ассет через систему byte sources.

## Основные концепции

### VoxelChunk

`VoxelChunk` владеет persistent `NativeArray`-буферами:

- `VoxelTypes`
- `MaterialIds`
- опционально `Colors`

Chunk нужно освобождать через `Dispose`, когда он больше не используется.

```csharp
using LedenevTV.Voxel;
using Unity.Mathematics;
using UnityEngine;

var chunk = new VoxelChunk(new int3(16, 16, 16), useColors: true);
chunk.TrySetVoxel(new Vector3Int(1, 1, 1), VoxelType.Solid, 0, Color.red);

chunk.Dispose();
```

### Генерация меша

`VoxelMeshBuilder` строит Unity `Mesh` из chunk. Он генерирует только видимые cube faces, группирует индексы по material submesh и может записывать vertex colors.

```csharp
using LedenevTV.Voxel;
using LedenevTV.Voxel.Drawing;
using Unity.Mathematics;
using UnityEngine;

var chunk = new VoxelChunk(new int3(16, 16, 16), useColors: true);
chunk.TrySetVoxel(new Vector3Int(4, 4, 4), VoxelType.Solid, 0, Color.white);

var builder = new VoxelMeshBuilder(
    new VoxelMeshSettings(8),
    new CenterChunkSpace());

Mesh mesh = builder.RebuildMesh(new Mesh(), chunk, drawFacesOnBounds: true);
```

### Runtime loading

Runtime loading построен вокруг byte-source интерфейсов:

- `IBytesSource`
- `IAsyncBytesSource`
- `AsyncBytesSource`

Готовые источники данных:

- `VoxelAssetBytesSource`
- `StreamingAssetsBytesSource`
- `AddressablesVoxelBytesSource`
- `WebRequestBytesSourceAsset`

`LazyChunkProvider` и `AsyncLazyChunkProvider` кэшируют загруженные chunks и сгенерированные meshes.

### Редактирование

Редактирование состоит из mask creator и applier. Например, `SphereMaskCreator` выбирает воксели внутри сферы, а `ChunkMaskApplier` меняет их type, material и color.

```csharp
using LedenevTV.Voxel;
using LedenevTV.Voxel.Editing;
using UnityEngine;

var mask = new SphereMaskCreator(new Vector3Int(8, 8, 8), radius: 3f);

chunkMaskApplier.Apply(chunk, mask, new MaskApplySettings
{
    IsModifyVoxelType = true,
    IsModifyMaterialId = true,
    IsModifyColors = true,
    VoxType = VoxelType.Solid,
    MaterialId = 0,
    Color = Color.green
});
```

### Splitting

`ConnectedComponentsChunkSplitter` разбивает solid и transparent voxels на связанные части. Соседство настраивается через:

- `NeighborVoxels6`
- `NeighborVoxels18`
- `NeighborVoxels26`

Демо-installer `VoxelSplitInstaller` позволяет выбрать режим в Inspector.

## Zenject installers

Runtime-системы собираются через installers из `Assets/Scripts/Runtime/Installers`:

- `VoxelDrawingInstaller`
- `VoxelSerializationMonoInstaller`
- `VoxelEditingInstaller`
- `VoxelSplitInstaller`
- `UnityBridgeInstaller`
- `LoadingSceneInstaller`

Добавьте нужные installers в scene context, чтобы связать сервисы, используемые примерами и runtime-компонентами.

## Запуск тестов

Откройте Unity Test Runner и запустите Edit Mode tests либо используйте batch mode:

```powershell
Unity.exe -batchmode -projectPath . -runTests -testPlatform editmode -testResults TestResults.xml -quit
```

Тесты находятся в `Assets/Scripts/Tests/EditModeTests`.

## Лицензия

Проект распространяется по MIT License. Подробности — в `LICENSE`.
