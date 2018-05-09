import _ from 'lodash';
import {
  reduxStore
} from './redux/contentScriptStore';
import {
  getNotesForUsers
} from './redux/actions/notes';

class SNListener {
  constructor() {
    this.started = false;
    this.boundFunc = this.listener.bind(this);
    this.authorsQueue = [];
    this.snMain = undefined;
  }
  start() {
    if (!this.started) {
      document.addEventListener('reddit', this.boundFunc, true);
      document.dispatchEvent(new CustomEvent('reddit.ready', {
        detail: {
          name: 'snoonotes'
        }
      }));
      this.started = true;
    }
  }
  listener(event) {
    const eventType = event.detail.type;
    //if(event.target.hasAttribute()); //escape unneeded duplicates

    if (eventType == 'postAuthor' || eventType == 'commentAuthor') {
      var snTarget = event.target.querySelector('span[data-name="snoonotes"]');
      if (snTarget.classList.contains('SNDone')) {
        return;
      }
      snTarget.classList.add('SNDone');
      if (this.authorsQueue.indexOf(event.detail.data.author) == -1) {
        this.authorsQueue.push(event.detail.data.author);
      }
      _.debounce(() => {
        getNotesForUsers(reduxStore.dispatch, this.authorsQueue);
        this.authorsQueue = [];
      }, 100, {
        maxWait: 1500
      });

      let url = 'https://reddit.com/r/' + event.detail.data.subreddit.name + '/' + event.detail.data.post.id.replace('t3_', '') + '/' + (eventType == 'commentAuthor' ? ('.../' + event.detail.data.comment.id.replace('t1_', '')) : '');
      let noteElemTarget = document.createElement('span');
      this.snMain.$refs.noteDisplay.injectNewUserNotesComponent(event.detail.data.author, event.detail.data.subreddit.name, url, noteElemTarget);

      snTarget.appendChild(noteElemTarget);

    }

  }
  stop() {
    if (this.started) {
      document.removeEventListener('reddit', this.boundFunc);
      this.started = false;
    }
  }
}

export let jsapiListener = new SNListener();
