# 单轨道列车系统使用指南

## 系统概述
这是一个简化的单轨道列车管理系统，列车在一条轨道上循环运行。

## 快速设置

### 1. 自动设置（推荐）
1. 在场景中创建空对象，命名为"SceneSetupHelper"
2. 添加 `SceneSetupHelper` 脚本组件
3. 运行场景，脚本会自动创建所有必要组件
4. 或按 `S` 键手动触发设置

### 2. 手动设置
如果自动设置失败，可以手动创建以下组件：

#### 必需组件：
- **MainThreadDispatcher**: 处理UDP消息的主线程调度器
- **UDPManager**: 处理UDP通信
- **TrainManager**: 列车管理器（备用系统）
- **TrackManager**: 单轨道管理器
- **TrainPrefab**: 列车预制件
- **SpawnPoint**: 列车生成点

## TrackManager 配置

### 基本设置：
- **Entry Point**: 列车进入轨道的起始点
- **Exit Point**: 列车离开轨道的终点（可选）
- **Max Trains**: 轨道上最大列车数量（默认30）
- **Track Animation**: 轨道动画片段
- **Animation Speed**: 动画播放速度
- **Loop Animation**: 是否循环播放动画

### 全局设置：
- **Max Global Trains**: 场景中最大列车总数（默认30）
- **Train Prefab**: 列车预制件
- **Show Debug Info**: 是否显示调试信息

## 使用方法

### 1. 发送UDP信号创建列车
```
{model1;D1:R}  // 创建红色列车
{model2;D1:B}  // 创建蓝色列车
```

### 2. 列车行为
- 列车从Entry Point生成
- 按照Track Animation定义的路径移动
- 如果Loop Animation开启，列车会循环运行
- 如果Loop Animation关闭，列车到达终点后会被销毁

### 3. 数量限制
- 单轨道最多30辆列车
- 全局最多30辆列车
- 超过限制时，最早的列车会被自动移除

## 调试功能

### 运行时GUI显示：
- 全局列车数量
- 轨道列车数量
- 循环模式状态
- 动画片段信息

### 控制台输出：
- 列车创建/移除日志
- 错误和警告信息
- 系统状态信息

## 故障排除

### 常见问题：

1. **"没有找到可用的列车管理器"**
   - 确保场景中有TrackManager组件
   - 使用SceneSetupHelper自动创建

2. **列车不移动**
   - 检查Track Animation是否设置
   - 确认TrainController组件存在
   - 验证动画控制器配置

3. **列车数量不更新**
   - 检查Max Trains和Max Global Trains设置
   - 确认Loop Animation设置

### 调试步骤：
1. 运行SceneSetupHelper的"检查组件状态"
2. 查看Console窗口的错误信息
3. 确认所有必要组件都已创建
4. 验证UDP信号格式正确

## 文件结构
```
Assets/
├── Scripts/
│   ├── TrackManager.cs          # 单轨道管理器
│   ├── TrainManager.cs          # 列车管理器（备用）
│   ├── UDPManager.cs            # UDP通信管理器
│   ├── MainThreadDispatcher.cs  # 主线程调度器
│   ├── TrainController.cs       # 列车控制器
│   ├── TrainData.cs             # 列车数据
│   └── SceneSetupHelper.cs      # 场景设置助手
├── Animations/
│   ├── Track1.anim              # 轨道动画
│   └── SingleTrackController.controller  # 单轨道动画控制器
└── Scenes/
    └── [你的场景文件]
```

## 扩展功能
如需添加更多功能，可以：
- 修改TrackManager添加多条轨道
- 扩展TrainController添加更多动画状态
- 自定义列车外观和行为
- 添加更多UDP命令支持


