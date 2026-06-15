export class PendingQuestion {
  constructor(data = {}) {
    Object.assign(this, {
      id: 0,
      playerId: 0,
      question: "",
      categoryNo: 0,
      answersJson: "[]",
      reported: 0,
      status: 0,
      remark: null,
      submittedAt: new Date().toISOString(),
      ...data
    });
  }
}
