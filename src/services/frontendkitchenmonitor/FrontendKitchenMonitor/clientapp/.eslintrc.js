module.exports = {
  root: true,                  
  parser: 'vue-eslint-parser', 
  parserOptions: {
    parser: '@babel/eslint-parser', 
    ecmaVersion: 2020,
    sourceType: 'module',
  },
  env: {
    browser: true,
    node: true,
    es2021: true,
  },
  extends: [
    'plugin:vue/essential', 
    'eslint:recommended'
  ],
  rules: {
    // your custom rules, for example:
    // 'no-console': process.env.NODE_ENV === 'production' ? 'warn' : 'off',
  },
};
