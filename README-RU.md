<p align="center">
<img width="400" src="https://github.com/user-attachments/assets/e2ae19e1-b121-46a2-94bc-eabf7378071b">
</p>

<p align="center">
<img alt="Version" src="https://img.shields.io/github/package-json/v/DCFApixels/DragonECS-Recursivity?color=%23ff4e85&style=for-the-badge">
<img alt="License" src="https://img.shields.io/github/license/DCFApixels/DragonECS-Recursivity?color=ff4e85&style=for-the-badge">
<a href="https://discord.gg/kqmJjExuCf"><img alt="Discord" src="https://img.shields.io/badge/Discord-JOIN-00b269?logo=discord&logoColor=%23ffffff&style=for-the-badge"></a>
<a href="http://qm.qq.com/cgi-bin/qm/qr?_wv=1027&k=IbDcH43vhfArb30luGMP1TMXB3GCHzxm&authKey=s%2FJfqvv46PswFq68irnGhkLrMR6y9tf%2FUn2mogYizSOGiS%2BmB%2B8Ar9I%2Fnr%2Bs4oS%2B&noverify=0&group_code=949562781"><img alt="QQ" src="https://img.shields.io/badge/QQ-JOIN-00b269?logo=tencentqq&logoColor=%23ffffff&style=for-the-badge"></a>
</p>

# Recursivity for [DragonECS](https://github.com/DCFApixels/DragonECS)

<table>
  <tr></tr>
  <tr>
    <td colspan="3">Readme Languages:</td>
  </tr>
  <tr></tr>
  <tr>
    <td nowrap width="100">
      <a href="https://github.com/DCFApixels/DragonECS-Recursivity/blob/main/README-RU.md">
        <img src="https://github.com/user-attachments/assets/3c699094-f8e6-471d-a7c1-6d2e9530e721"></br>
        <span>Русский</span>
      </a>  
    </td>
    <td nowrap width="100">
      <a href="https://github.com/DCFApixels/DragonECS-Recursivity">
        <img src="https://github.com/user-attachments/assets/30528cb5-f38e-49f0-b23e-d001844ae930"></br>
        <span>English(WIP)</span>
      </a>  
    </td>
  </tr>
</table>

</br>
  
Упрощает обработку событий симулируя поведение рекурсии, но в рамках стандартной структуры ECS. Гарантирует для событий которые должны быть обработаны в рамках одного кадра, что они будут обработаны всеми системами ровно один раз, в независимости от места появления.

> **ВАЖНО!** Проект в стадии разработки. API может меняться.

# Оглавление
- [Установка](#установка)
- [Инициализация](#инициализация)
- [Обработка событий](#обработка-событий)

</br>

# Установка
Семантика версионирования - [Открыть](https://gist.github.com/DCFApixels/e53281d4628b19fe5278f3e77a7da9e8#file-dcfapixels_versioning_ru-md)
## Окружение
Обязательные требования:
+ Зависимость: [DragonECS](https://github.com/DCFApixels/DragonECS)
+ Минимальная версия C# 7.3;

Опционально:
+ Игровые движки с C#: Unity, Godot, MonoGame и т.д.

Протестированно:
+ **Unity:** Минимальная версия 2020.1.0;

## Установка для Unity
* ### Unity-модуль
Поддерживается установка в виде Unity-модуля в  при помощи добавления git-URL [в PackageManager](https://docs.unity3d.com/2023.2/Documentation/Manual/upm-ui-giturl.html) или ручного добавления в `Packages/manifest.json`: 
```
https://github.com/DCFApixels/DragonECS-Recursivity.git
```
* ### В виде иходников
Фреймворк так же может быть добавлен в проект в виде исходников.

</br>

# Инициализация
Для обработки событий используется процесс `IOn<T>.ToRun()` где `T` это тип компонента-события. Процесс `IOn<T>.ToRun()` контролирует специальная система, которую необходимо инициализировать в пайплайне.
``` c#
_world = new EcsDefaultWorld();
_pipeline = EcsPipeline.New()
    // ...
    // Инициализация системы контролирующей процесс IOn<DamageEvent>.ToRun().
    // Аргумент maxLoops устанавливает лимит на количество выполнений за один кадр.
    .AddOn<DamageEvent>(maxLoops: 100)
    // Добавление систем обрабатывающих этот процесс
    .Add(new SomeDamageSystem())
    .Add(new SomeReturnDamageAbilitySystem())
    // ...
    .Inject(_world)
    .BuildAndInit();
```

</br>

# Обработка событий
Системы с `IOn<T>.ToRun()` будут выполняться до тех пор пока в мире остается хотя бы один компонент-событие `T`. 

С точки зрения замедления производительности влияние не высоко по нескольким причинам:
* Системы с `IOn<T>.ToRun()` не выполняются вовсе если в мире нет ни одного компонента `T`.
* В инициализации `.AddOn<DamageEvent>(maxLoops)` Можно выставить лимит(maxLoops) вызова `IOn<T>.ToRun()`, тогда при достижении лимита, оставшиеся события будут обработаны в следующем кадре.

> Имеется защита от бесконечного зацикливания в виде глобального лимита в `100_000` повторений.

Ниже приведен пример системы обрабатывающей события. В примере реализована система применения урона к здоровью и система способности возврата урона атакующему, что-то вроде шипов.

<details>
<summary>Используемые в примере компоненты</summary>

``` c#
using DCFApixels.DragonECS;
public struct Health : IEcsComponent
{
    public float points;
}
public struct DamageEvent : IEcsComponent
{
    public entlong source;
    public entlong target;
    public float points;
}
public struct ReturnDamageAbility : IEcsComponent
{
    public float multiplier;
}
```

</details>

> Этот пример имеет некоторые проблемы, но как пример достаточно нагляден для понимания работы.


``` c#
// Система которая применяет полученный урон к здоровью.
public class SomeApplyDamageSystem : IOn<DamageEvent>, IEcsInject<EcsDefaultWorld>
{
    class EventAspect : EcsAspect
    {
        public EcsPool<DamageEvent> damageEvents = Inc;
    }
    class Aspect : EcsAspect
    {
        public EcsPool<Health> healths = Inc;
    }

    EcsDefaultWorld _world;

    // targetEntities содержит все сущности с компонентом DamageEvent. 
    // Сущности из этого же списка в конце цикла будут автоматически отчищены от компонента DamageEvent.
    void IOn<DamageEvent>.ToRun(EcsSpan targetEntities)
    {
        var a = _world.GetAspect<Aspect>();
        // Итерироваться нужно по targetEntities, 
        // так можно гарантировать что системы будут обрабатывать каждое событие один раз.
        foreach (var ee in targetEntities.Where(out EventAspect ea))
        {
            ref var damageEvent = ref ea.damageEvents.Get(ee);
            // Извлечение ID сущности с проверкой что она не была удалена.
            // И проверка на соответствие аспекту Aspect.
            if (damageEvent.target.TryGetID(out int e) && 
                _world.IsMatchesMask(a, e))
            {
                ref var health = ref a.healths.Get(e);
                health.points -= damageEvent.points;
            }
        }
    }

    public void Inject(EcsDefaultWorld obj) => _world = obj;
}
```
``` c#
// Система которая делает возвратный урон.
public class SomeReturnDamageAbilitySystem : IOn<DamageEvent>, IEcsInject<EcsDefaultWorld>
{
    class EventAspect : EcsAspect
    {
        public EcsPool<DamageEvent> damageEvents = Inc;
    }
    class Aspect : EcsAspect
    {
        public EcsPool<ReturnDamageAbility> returnDamageAbilities = Inc;
    }

    EcsDefaultWorld _world;

    void IOn<DamageEvent>.ToRun(EcsSpan targetEntities)
    {
        var a = _world.GetAspect<Aspect>();
        // Итерируемся по targetEntities.
        foreach (var ee in targetEntities.Where(out EventAspect ea))
        {
            ref var damageEvent = ref ea.damageEvents.Get(ee);
            if (damageEvent.target.TryGetID(out int targetE) &&
                damageEvent.source.TryGetID(out int sourceE) &&
                _world.IsMatchesMask(a, targetE))
            {
                ref var returnDamageAbility = ref a.returnDamageAbilities.Get(targetE);

                // Создание события возвратного урона,
                // которое будет обработано в следующем цикле.
                int newEE = _world.NewEntity(ea);
                ref var newDamageEvent = ref ea.damageEvents.Get(newEE);
                newDamageEvent.target = damageEvent.source;
                newDamageEvent.source = damageEvent.target;
                newDamageEvent.points = damageEvent.points * returnDamageAbility.multiplier;
            }
        }
    }

    public void Inject(EcsDefaultWorld obj) => _world = obj;
}
```