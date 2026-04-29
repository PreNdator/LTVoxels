# LTVoxels

Система работы с вокселями для Unity: генерация мешей на Job и редактирование чанков без GC-аллокаций, разбиение чанков, сериализация/десериализация данных, импорт .vox (MagicaVoxel). Включает примеры использования и unit-тесты.


## Демо-сцены

| Сцена | Назначение |
| --- | --- |
| `Assets/Scenes/Main.unity` | Стартовая сцена. |
| `Assets/Scenes/ChunkLoad.unity` | Загрузка voxel assets и построение chunk meshes. |
| `Assets/Scenes/ChunkEditing.unity` | Runtime-редактирование чанка движущейся sphere mask. |
| `Assets/Scenes/ChunkSplit.unity` | Разбиение чанка на связанные части и обратное объединение. |

## Поддерживаемые voxel inputs

### `.vox`

MagicaVoxel `.vox` импортируется через `MagicaVoxelVoxImporter`.

### `.ply`

ASCII PLY point cloud в формате экспорта MagicaVoxel импортируется через `PlyPointCloudImporter`.

### `.voxch`

`.voxch` — внутренний формат chunk, реализованный в `VoxchImportExport`.

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

`ConnectedComponentsChunkSplitter` разбивает непустые воксели на связанные части. Соседство настраивается через:

- `NeighborVoxels6`
- `NeighborVoxels18`
- `NeighborVoxels26`

Демо-installer `VoxelSplitInstaller` позволяет выбрать режим в Inspector.
