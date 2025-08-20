# 列车动画状态机系统使用指南

## 系统概述

本系统使用Unity的Animator状态机来控制列车在三条轨道上的运动，实现了以下功能：

- 三条独立的轨道（Track1、Track2、Track3）
- 列车按照 轨道1 → 轨道2 → 轨道3 → 轨道1 的顺序循环运动
- 每条轨道最多10辆列车
- 整个场景最多30辆列车
- 支持UDP信号控制列车创建和颜色配置

## 系统组件

### 1. 核心脚本

#### TrackManager.cs
- 管理三条轨道的列车分配
- 控制列车数量限制
- 处理列车在轨道间的转移
- 提供调试信息显示

#### TrainController.cs
- 控制单个列车的动画状态
- 管理动画状态切换
- 支持动画暂停/恢复
- 提供动画事件支持

#### TrainAnimationTester.cs
- 提供测试界面
- 支持手动和自动创建列车
- 提供列车控制功能

### 2. 动画资源

#### 动画片段
- `Track1.anim`: 轨道1的运动路径
- `Track2.anim`: 轨道2的运动路径  
- `Track3.anim`: 轨道3的运动路径

#### 动画控制器
- `TrainAnimationController.controller`: 主动画状态机

## 设置步骤

### 1. 场景设置

1. 在场景中创建三个空GameObject作为轨道入口点：
   - Track1Entry
   - Track2Entry  
   - Track3Entry

2. 添加TrackManager组件到场景中的GameObject

3. 在TrackManager的Inspector中配置：
   - 设置trainPrefab为你的列车预制体
   - 配置三个轨道的entryPoint
   - 设置动画片段（Track1.anim、Track2.anim、Track3.anim）
   - 调整maxTrains和maxGlobalTrains

### 2. 列车预制体设置

1. 确保列车预制体包含以下组件：
   - Animator组件
   - TrainController脚本
   - 列车模型

2. 在Animator组件中：
   - 设置Controller为TrainAnimationController
   - 确保动画片段正确绑定

3. 在TrainController组件中：
   - 设置animator引用
   - 启用enableAnimationEvents

### 3. 动画状态机配置

动画状态机包含三个状态：
- Track1: 对应轨道1的动画
- Track2: 对应轨道2的动画  
- Track3: 对应轨道3的动画

状态转换使用触发器：
- ToTrack1: 切换到轨道1
- ToTrack2: 切换到轨道2
- ToTrack3: 切换到轨道3

## 使用方法

### 1. 通过代码创建列车

```csharp
// 创建基本列车
GameObject train = TrackManager.Instance.CreateTrain();

// 创建带配置的列车
GameObject train = TrackManager.Instance.CreateTrain("{model1;D1:R,D2:G,D3:B}");
```

### 2. 通过UDP信号创建列车

系统会自动监听UDP消息，当收到格式为`{model;D1:color,D2:color}`的消息时，会自动创建列车。

### 3. 手动控制列车

```csharp
// 暂停列车动画
TrainController controller = train.GetComponent<TrainController>();
controller.PauseAnimation();

// 恢复列车动画
controller.ResumeAnimation();

// 切换到指定轨道
controller.SwitchToTrack("Track2");
```

## 配置参数

### TrackManager配置

- **maxGlobalTrains**: 全局最大列车数量（默认30）
- **track1/2/3.maxTrains**: 每条轨道的最大列车数量（默认10）
- **track1/2/3.entryPoint**: 轨道入口点Transform
- **track1/2/3.animationClip**: 轨道动画片段
- **showDebugInfo**: 是否显示调试信息

### TrainController配置

- **animator**: Animator组件引用
- **currentTrack**: 当前轨道名称
- **enableAnimationEvents**: 是否启用动画事件

## 动画事件

系统支持以下动画事件：

- `OnTrackCompleted()`: 当前轨道动画完成时调用
- `OnTrackStarted()`: 轨道动画开始时调用

这些事件会自动触发列车转移到下一个轨道。

## 调试和测试

### 1. 使用TrainAnimationTester

添加TrainAnimationTester组件到场景中，可以：
- 手动创建测试列车
- 自动创建列车
- 暂停/恢复所有列车
- 查看系统状态

### 2. 调试信息

启用TrackManager的showDebugInfo可以在屏幕上显示：
- 全局列车数量
- 每条轨道的列车数量
- 系统状态信息

### 3. 控制台日志

系统会输出详细的日志信息：
- 列车创建和销毁
- 轨道切换
- 错误和警告信息

## 故障排除

### 常见问题

1. **列车不移动**
   - 检查动画片段是否正确设置
   - 确认Animator组件已配置
   - 验证动画状态机设置

2. **列车不切换轨道**
   - 检查enableAnimationEvents是否启用
   - 确认动画事件正确配置
   - 验证TrackManager实例存在

3. **UDP信号无响应**
   - 确认UDPManager存在且正确配置
   - 检查网络端口设置
   - 验证消息格式正确

4. **列车数量限制不生效**
   - 检查maxTrains和maxGlobalTrains设置
   - 确认TrackManager正确初始化
   - 验证列车清理逻辑

### 性能优化

1. 定期清理无效的列车引用
2. 合理设置列车数量限制
3. 优化动画片段大小和复杂度
4. 使用对象池管理列车实例

## 扩展功能

### 1. 添加新轨道

1. 创建新的动画片段
2. 在动画状态机中添加新状态
3. 更新TrackManager的轨道配置
4. 修改状态转换逻辑

### 2. 自定义动画事件

在动画片段中添加自定义事件，在TrainController中实现对应方法。

### 3. 动态轨道配置

可以通过代码动态修改轨道参数，如最大列车数量、动画速度等。

## 版本信息

- Unity版本: 2021.3+
- 系统版本: 1.0
- 最后更新: 2024年

## 技术支持

如有问题或建议，请查看控制台日志或联系开发团队。
