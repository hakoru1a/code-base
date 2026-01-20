const stringHelper = {
  insertWhitespacePascal(value: string) {
    if (!value) return '';
    return value.replace(/([A-Z])/g, ' $1').trim();
  },
  newGuid() {
    return 'xxxxxxxx-xxxx-4xxx-yxxx-xxxxxxxxxxxx'.replace(/[xy]/g, (c) => {
      const r = (Math.random() * 16) | 0,
        v = c == 'x' ? r : (r & 0x3) | 0x8;
      return v.toString(16);
    });
  }
};

export default stringHelper;
