<template>
    <div class="sn-paginator">
        <span class="sn-paginator-label">Rows: </span>
        <span class="sn-paginator-rowopt" :class="{active: rows == 10}" @click="setRows(10)">10</span>
        <span class="sn-paginator-rowopt" :class="{active: rows == 25}" @click="setRows(25)">25</span>
        <span class="sn-paginator-rowopt" :class="{active: rows == 50}" @click="setRows(50)">50</span>

        <span class="sn-paginator-details">{{((page - 1) * rows) + 1}}-{{pageTotal}} of {{total}}</span>

        <input type="button" class="sn-paginator-prev" @click.native="changePage(page - 1)" :disabled="page === 1"></input>
        <input type="button" class="sn-paginator-next" @click.native="changePage(page + 1)" :disabled="disableNext"></input>
    </div>
</template>
<script>
    export default{
        props:{
            rows:{
                type: Number,
                default 25
            },
            page:{
                type: Number,
                default 1
            },
            total:{
                type: Number,
                default 0
            }
        },
        computed:{
            pageTotal(){
                return Math.min(this.page * this.rows, this.total);
            },
            disableNext(){
                return this.page * this.rows >= this.total;
            }
        },
        methods:{
            emitPageinatorEvent(r, p){
                this.$emit('pagination',{
                    rows: r,
                    page: p
                })
            }
            setRows(val){
                this.emitPageinatorEvent(val, this.page);
            }
            changePage(p){
                this.emitPageinatorEvent(this.rows, p);
            }
        }
    }
</script>