
![Unity](https://img.shields.io/badge/Unity-2022.3%2B-black)

> [!IMPORTANT]
> このプラグインはデータの復元を行いますが、セーブデータの作成機能は含まれていません。

# HistoryTracker
セーブデータ(永続保存データ)を復元する、Unity向けのプラグインです。

![HistoryTracker](Docs/header.png)

## Features
- セーブデータ(永続保存データ)の保存
- セーブデータの復元
- 見やすいUIで管理
- Editorで保存したデータを実機で復元

## Why HistoryTracker?

### 1. 確率要素（ガチャ・ドロップ）の集中デバッグ
「1%の確率でドロップするアイテム」や「クリティカルヒット時の挙動」を確認したい場合、ゲームを最初からやり直すのは時間の無駄です。 判定が行われる直前の状態で保存し、判定後にロードして何度も試行することで、低確率イベントの挙動確認を短時間で何十回も行えます。

### 2. クエスト分岐・マルチエンディングの網羅テスト
RPGやアドベンチャーゲームで、「選択肢Aを選んだ場合」と「選択肢Bを選んだ場合」の両方をテストしたい時に役立ちます。 分岐ポイントの直前で状態を保存しておけば、一方のルートを確認した後、即座に分岐前に戻ってもう一方のルートを確認でき、デバッグの手戻りを最小限に抑えられます。

### 3. QA（品質保証）時の「バグ再現」ツールとして
テスターがバグを発見した際、「どうやってその状態になったか」を再現するのは困難です。 このプラグインを使い、**定期的に保存**しておけば、バグが発生した瞬間に巻き戻して、開発者に正確な発生状況を共有したり、再現手順を特定したりすることが容易になります。

### 4. ゲームバランス調整（パラメータ調整）の高速化
ボス戦の難易度調整などで、「攻撃力を少し上げてリトライしたい」という場面です。 戦闘開始前の状態を保持しておき、インスペクターで敵のパラメータを調整しては「復元して再戦」を繰り返すことで、理想のゲームバランスを探ることができます。

### 5. テストの初期化処理
統合テストを行う際、テストごとに「所持金1000G、レベル10の状態」を作る必要があります。 毎回新規データからセットアップするのではなく、あらかじめ作っておいた「理想の状態」をこのプラグインで復元してテストを開始すれば、テストの実行時間を大幅に短縮し、クリーンな環境でテストを行えます。

## Getting Started

### Install from Git URL

"Unity Editor : Window > Package Manager > Add package from git URL...".

```
https://github.com/IShix-g/HistoryTracker.git?path=Packages/HistoryTracker
```

<img src="Docs/add_package.png" width="850"/>

## Scripts

### 実装

セーブシステムとHistoryTrackerを関連付ける`IHistSaveDataHandler`を実装します。

| メソッド               | 説明                                                                              |
|--------------------|---------------------------------------------------------------------------------|
| OnBeforeSave()     | セーブする直前に呼ばれます。返り値でタイトルと説明を返します。(`HistRecordInfo`)。この内容はUIに表示されます                |
| GetSaveFilePaths() | セーブデータのフルパスを配列で返します。例) `Application.persistentDataPath` + "/data.bytes"           
| ApplyData()        | セーブデータが復元された後に呼ばれます。`Application.Quit()`などを呼んで一度アプリを閉じるなどの処理を追加してセーブデータを反映させます。 |

```csharp
using HistoryTracker;

public sealed class TestModelRepository : ModelRepository, IHistSaveDataHandler
{
    public HistRecordInfo OnBeforeSave()
    {
        // Save data
        for (var i = 0; i < Models.Count; i++)
        {
            var model = Models[i];
            var path = GetFullPath(model.Id);
            Save(model, path);
        }
        // Return the title and description
        var title = "Saved Count: " + Models[0].SaveCount;
        var description = "[Test]";
        return new HistRecordInfo(title, description);
    }

    // Determine and return the file path
    // e.g. `Application.persistentDataPath` + "/data.bytes" 
    public IReadOnlyList<string> GetSaveFilePaths() => Paths.Values.ToList();

    public void ApplyData() => Restored();
}
```

### HistRecordInfo

上記の`OnBeforeSave()`で設定したタイトルと説明は、UIで下記のように表示されます。

<img src="Docs/hist_record_info.png" width="900"/>

### 依存の設定

上記で実装した`IHistSaveDataHandler`をHistoryTrackerに設定します。Awakeなどできる限り早いタイミングで登録してください。

```csharp
void Awake()
{
    // Initialize the repository that implements IHistSaveDataHandler in Awake
    Hist.Configure(_repository);
}
```

### ダイアログを開く

スクリプトからは下記のコードで開きます。必要無くなったタイミングで`Hist.Release()`で解放する事も可能ですが、とても軽いUIオブジェクトなので問題になる可能性は低いです。

```csharp
using HistoryTracker;

void OnDialogButtonClicked()
{
    // Display a dialog by creating an object and calling OpenDialog.
    var obj = Hist.CreateOrGet();
    obj.OpenDialog(() =>
    {
        // You can also release it when the dialog closes.
        // Hist.Release();
    });
}
```

## ダイアログの説明

### 一覧

<img src="Docs/dialog1.jpg" width="450"/>

| No | 説明                    |
|----|-----------------------|
| ①  | セーブデータを保存するボタン        |
| ②  | 件数の表示                 |
| ③  | 保存したセーブデータ。クリックで詳細を開く |
| ④  | 次のページへ                |
| ⑤  | 閉じる                   |

### 詳細

<img src="Docs/dialog2.jpg" width="450"/>

| No | 説明                  |
|----|---------------------|
| ①  | セーブデータの復元ボタン           |
| ②  | セーブデータの削除ボタン           |
| ③  | タイトル 長押しで編集 *       |
| ④  | 説明 長押しで編集 *         |
| ⑤  | 自動的に、生成したパスが表示されます。 |
| ⑥  | Editorで生成した場合、表示される |
| ⑦  | 生成日                 |
| ⑧  | 閉じる                 |

* 実機の場合、Editorで生成したデータを編集できません。
