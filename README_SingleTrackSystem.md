# 单轨道列车系统使用说明

## 系统概述

单轨道列车系统是一个简化的列车管理系统，只使用Track1轨道，实现以下功能：
- 接收UDP信号后生成列车
- 列车在Track1上循环移动
- 限制最多30辆列车
- 超过30辆时，最新列车替换最早生成的列车

## 核心组件

### 1. SingleTrackManager
**位置**: `Assets/Scripts/SingleTrackManager.cs`
**功能**: 管理单轨道列车系统
**主要特性**:
- 列车创建和销毁
- 列车数量限制（最多30辆）
- 自动替换最旧列车
- 颜色配置处理

### 2. SingleTrackTrainController
**位置**: `Assets/Scripts/SingleTrackTrainController.cs`
**功能**: 控制单个列车的动画和移动
**主要特性**:
- 只支持Track1轨道
- 循环动画播放
- UDP信号启动
- 动画事件处理

### 3. SingleTrackTester
**位置**: `Assets/Scripts/SingleTrackTester.cs`
**功能**: 测试和调试单轨道系统
**主要特性**:
- 手动创建测试列车
- 批量创建测试
- 系统状态监控
- 功能验证

## 设置步骤

### 步骤1: 场景设置
1. 在场景中创建空GameObject，命名为"SingleTrackManager"
2. 添加 `SingleTrackManager` 组件
3. 设置以下参数：
   - **Track1 Start**: Track1轨道的起点Transform
   - **Train Prefab**: 列车预制体（必须包含SingleTrackTrainController组件）
   - **Max Trains**: 最大列车数量（默认30）
   - **Animation Speed**: 动画速度（默认1.0）

### 步骤2: 列车预制体设置
1. 确保列车预制体包含以下组件：
   - `SingleTrackTrainController`
   - `Animator`（动画控制器）
2. 在Animator中设置：
   - 动画状态：Track1
   - 触发器：ToTrack1
   - 动画事件：OnTrackCompleted（在动画结束时调用）

### 步骤3: 动画设置
1. 创建Track1动画片段
2. 在动画片段末尾添加事件：
   - 函数名：`OnTrackCompleted`
   - 时间：动画结束前0.1秒
3. 确保动画设置为循环播放

### 步骤4: UDP设置
1. 确保场景中有UDPManager组件
2. UDPManager会自动检测SingleTrackManager并优先使用

## 使用方法

### 通过UDP信号创建列车
发送UDP信号格式：`{model1;D1:R,D2:G,D3:B}`
- 系统会自动创建列车
- 应用颜色配置
- 启动列车动画

### 通过测试器创建列车
1. 在场景中添加 `SingleTrackTester` 组件
2. 使用GUI界面或键盘快捷键：
   - 空格键：创建单个测试列车
   - B键：批量创建10辆列车

## 系统特性

### 列车数量管理
- 自动限制最多30辆列车
- 超过限制时自动移除最旧的列车
- 使用队列（Queue）确保先进先出

### 循环移动
- 列车在Track1上无限循环移动
- 动画完成后自动重新开始
- 支持暂停和恢复功能

### 颜色配置
- 支持6种预设颜色：R,G,B,Y,C,M
- 自动创建TrainManager（如果不存在）
- 备用颜色应用机制

### 调试功能
- 实时GUI显示系统状态
- 详细的Console日志
- 测试工具集成

## 故障排除

### 问题1: 列车不移动
**可能原因**:
- SingleTrackTrainController未正确附加到预制体
- 动画事件未正确设置
- 动画控制器配置错误

**解决方法**:
1. 检查列车预制体是否包含SingleTrackTrainController
2. 验证动画事件OnTrackCompleted是否正确设置
3. 确认Animator状态机配置

### 问题2: 列车数量不限制
**可能原因**:
- SingleTrackManager未正确初始化
- 列车队列管理出错

**解决方法**:
1. 检查SingleTrackManager.Instance是否存在
2. 使用SingleTrackTester验证系统状态
3. 查看Console日志确认列车创建和销毁

### 问题3: 颜色不应用
**可能原因**:
- TrainManager未找到或未正确初始化
- 颜色配置格式错误

**解决方法**:
1. 检查TrainManager.Instance状态
2. 验证UDP信号格式
3. 查看Console中的颜色应用日志

## 性能优化

### 内存管理
- 自动清理无效的列车引用
- 及时销毁被替换的列车
- 使用对象池模式（可选扩展）

### 动画优化
- 使用动画事件而不是Update检查
- 合理设置动画速度
- 避免频繁的Transform操作

## 扩展功能

### 自定义颜色
可以通过修改TrainManager来添加更多颜色选项

### 多轨道支持
如果需要多轨道，可以扩展SingleTrackManager为MultiTrackManager

### 列车类型
可以添加不同的列车预制体支持

## 注意事项

1. **组件依赖**: 确保所有必需的组件都正确附加
2. **动画设置**: 动画事件必须在正确的时机触发
3. **UDP格式**: 严格按照指定格式发送UDP信号
4. **性能监控**: 注意列车数量对性能的影响
5. **调试模式**: 开发时建议启用所有调试选项

